#nullable enable
using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

internal class DbLogger : ILogger
{
    private readonly DbLoggerBuffer _loggerBuffer;
    private readonly IOptionsMonitor<DbLoggerOptions> _options;

    public DbLogger(DbLoggerBuffer loggerBuffer, IOptionsMonitor<DbLoggerOptions> options)
    {
        _loggerBuffer = loggerBuffer;
        _options = options;
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;
        
        var now = DateTime.Now;


        var message = GetMessage(eventId, formatter(state, exception), exception);
        
        var entry = new DbLogEntry
        {
            Created = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, now.Millisecond,
                now.Kind),
            LogLevel = (int)logLevel,
            Event = eventId.Name,
            Message = message
        };
        
        _loggerBuffer.Enqueue(entry.ToSeparatedCharArray());
    }
    
    private static string GetMessage(EventId eventId, string formatterMessage, Exception? exception)
    {
        var message = new StringBuilder();

        message.AppendLine(eventId.Name);
        message.AppendLine(formatterMessage);

        if (exception != null)
        {
            message.AppendLine("Message:");
            message.AppendLine(exception.Message);
            message.AppendLine("Stacktrace:");
            message.AppendLine(exception.StackTrace);
            message.AppendLine("Source:");
            message.AppendLine(exception.Source);
        }

        return message.ToString();
    }
}