using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Logging.File;

public class FileLogger : ILogger
{
    private readonly FileLoggerBuffer _buffer;
    private readonly IOptionsMonitor<FileLoggerOptions> _options;

    /// <summary>
    /// Creates a new instance of <see cref="FileLogger" />.
    /// </summary>
    public FileLogger(FileLoggerBuffer buffer, IOptionsMonitor<FileLoggerOptions> options)
    {
        _buffer = buffer;
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

        var message = new LogEntry<TState>(logLevel, eventId, state!, exception, (s, e) => formatter(s, e));
        var record = GetLogRecord<TState>(message, _options.CurrentValue.Formatting);
        _buffer.Enqueue(record.ToCharArray());
    }
    
    private static string GetLogRecord<TState>(LogEntry<TState> entry, FileLoggerFormatting formatting)
    {
        var log = new StringBuilder();

        switch (formatting)
        {
            case FileLoggerFormatting.Default:

                log.Append(DateTime.Now);
                log.Append(" ");
                if (!string.IsNullOrWhiteSpace(entry.EventId.Name))
                {
                    log.AppendFormat(" [{0}] ", entry.EventId.Name);
                }

                log.Append("(");
                log.Append(entry.LogLevel.ToString());
                log.AppendLine(")");
                log.Append(entry.Formatter(entry.State, entry.Exception));
                log.AppendLine();
                log.AppendLine();
                break;
            case FileLoggerFormatting.Compact:
            {
                log.AppendFormat("{0:yyyy-MM-dd HH:mm:ss+00:00} -", DateTime.Now);
                log.AppendFormat(" [{0}] ", entry.LogLevel);

                if (!string.IsNullOrWhiteSpace(entry.EventId.Name))
                {
                    log.AppendFormat(" [{0}] ", entry.EventId.Name);
                }

                log.AppendFormat(" {0} ", entry.Formatter(entry.State, entry.Exception));

                if (entry.Exception != null)
                {
                    log.AppendLine(entry.Exception.Message);
                    log.AppendLine(entry.Exception.StackTrace);
                    log.AppendFormat("Source: {0}", entry.Exception.Source);
                }
                log.AppendLine();
                break;
            }
            case FileLoggerFormatting.Json:
                log.Append(
                    JsonConvert.SerializeObject(new
                    {   Date = DateTime.Now,
                        Event = entry.EventId.Name,
                        entry.LogLevel,
                        Message = entry.Formatter(entry.State, entry.Exception),
                        entry.Exception
                    }, new JsonSerializerSettings
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