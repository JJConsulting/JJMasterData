#nullable enable

using Microsoft.Extensions.Logging;
using System;
using System.Text;

namespace JJMasterData.Commons.Logging.Db;

internal class DbLogger(string categoryName, LoggerBuffer loggerBuffer) : ILogger
{
    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = GetMessage(eventId, formatter(state, exception), exception);
        var entry = new LogMessage
        {
            Created = DateTime.Now,
            LogLevel = (int)logLevel,
            Event = eventId.Name ?? string.Empty,
            Message = message,
            Category = categoryName
        };
        
        loggerBuffer.Enqueue(entry);
    }

    private static string GetMessage(EventId eventId, string formatterMessage, Exception? exception)
    {
        var message = new StringBuilder();
        if (eventId.Name != null)
        {
            message.AppendLine($"Event - {eventId}");
        }

        message.AppendLine(formatterMessage);
        
        if (exception != null)
        {
            message.AppendLine(LoggerDecoration.GetMessageException(exception));
        }

        return message.ToString();
    }


}