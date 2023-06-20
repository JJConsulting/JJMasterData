using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Logging.File;

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    private readonly IOptionsMonitor<FileLoggerOptions> _options;
    private readonly BlockingCollection<LogMessage> _queue;

    public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
    {
        _queue = new BlockingCollection<LogMessage>();
        _options = options;

        Task.Factory.StartNew(LogAtFile, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Creates a new instance of the file logger.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(this);
    }

    internal void AddToQueue(LogMessage logMessage)
    {
        _queue.Add(logMessage);
    }

    private void LogAtFile()
    {
        foreach (var message in _queue.GetConsumingEnumerable())
        {
            var path = FileIO.ResolveFilePath(_options.CurrentValue.FileName);
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            var formatting = _options.CurrentValue.Formatting;
            var record = GetLogRecord(message, formatting);
            
            using var writer = new StreamWriter(path, true);
            writer.Write(record);
        }
    }

    private static string GetLogRecord(LogMessage message, FileLoggerFormatting formatting)
    {
        var log = new StringBuilder();

        switch (formatting)
        {
            case FileLoggerFormatting.Default:

                log.Append(DateTime.Now);
                log.Append(" ");
                if (!string.IsNullOrWhiteSpace(message.EventId.Name))
                {
                    log.AppendFormat(" [{0}] ", message.EventId.Name);
                }

                log.Append("(");
                log.Append(message.LogLevel.ToString());
                log.AppendLine(")");
                log.Append(message.Formatter(message.State, message.Exception));
                log.AppendLine();
                log.AppendLine();
                break;
            case FileLoggerFormatting.Compact:
            {
                log.AppendFormat("{0:yyyy-MM-dd HH:mm:ss+00:00} -", DateTime.Now);
                log.AppendFormat(" [{0}] ", message.LogLevel);

                if (!string.IsNullOrWhiteSpace(message.EventId.Name))
                {
                    log.AppendFormat(" [{0}] ", message.EventId.Name);
                }

                log.AppendFormat(" {0} ", message.Formatter(message.State, message.Exception));

                if (message.Exception != null)
                {
                    log.AppendLine(message.Exception.Message);
                    log.AppendLine(message.Exception.StackTrace);
                    log.AppendFormat("Source: {0}", message.Exception.Source);
                }
                log.AppendLine();
                break;
            }
            case FileLoggerFormatting.Json:
                log.Append(
                    JsonConvert.SerializeObject(new
                    {   Date = DateTime.Now,
                        Event = message.EventId.Name,
                        message.LogLevel,
                        Message = message.Formatter(message.State, message.Exception),
                        message.Exception
                    }, new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    }));
                log.AppendLine(",");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(formatting), formatting, null);
        }

        return log.ToString();
    }

    public void Dispose()
    {
    }
}