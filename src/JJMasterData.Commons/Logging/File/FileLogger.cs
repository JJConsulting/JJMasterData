using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Logging.File;

public class FileLogger : ILogger
{

    private readonly BlockingCollection<LogMessage> _queue;
    private readonly IOptionsMonitor<FileLoggerOptions> _optionsMonitor;
    private readonly FileLoggerOptions _options;

    private FileLoggerOptions Options
    {
        get
        {
            if (_options != null)
                return _options;

            return _optionsMonitor.CurrentValue;
        }
    }

    private FileLogger()
    {
        _queue = new BlockingCollection<LogMessage>();
        Task.Factory.StartNew(LogAtFile, TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// Creates a new instance of <see cref="FileLogger" />.
    /// </summary>
    public FileLogger(IOptionsMonitor<FileLoggerOptions> options) : this()
    {
        _optionsMonitor = options;
    }

    public FileLogger(FileLoggerOptions options) : this()
    {
        _options = options;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    /// <summary>
    /// Whether to log the entry.
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns></returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <summary>
    /// Used to log the entry.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logLevel">An instance of <see cref="LogLevel"/>.</param>
    /// <param name="eventId">The event's ID. An instance of <see cref="EventId"/>.</param>
    /// <param name="state">The event's state.</param>
    /// <param name="exception">The event's exception. An instance of <see cref="Exception" /></param>
    /// <param name="formatter">A delegate that formats </param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = new LogMessage(logLevel, eventId, state!, exception, (s, e) => formatter((TState)s, e));
        _queue.Add(message);
    }
    
    private void LogAtFile()
    {
        var options = Options;
        foreach (var message in _queue.GetConsumingEnumerable())
        {
            var path = FileIO.ResolveFilePath(options.FileName);
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory!);

            var formatting = options.Formatting;
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
}