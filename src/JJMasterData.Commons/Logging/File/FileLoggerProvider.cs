using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

            var record = GetLogRecord(message);
            
            using var writer = new StreamWriter(path, true);
            writer.Write(record);
        }
    }
    private static string GetLogRecord(LogMessage message)
    {
        var log = new StringBuilder();

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

        return log.ToString();
    }

    public void Dispose(){}
}