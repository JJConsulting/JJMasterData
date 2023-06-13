#nullable enable

using System;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

internal class LogMessage
{
    public LogLevel LogLevel { get; }
    public EventId EventId { get; }
    public object State { get; }
    public Exception? Exception { get; }
    public Func<object, Exception?, string> Formatter { get; }

    public LogMessage(LogLevel logLevel, EventId eventId, object state, Exception? exception, Func<object, Exception?, string> formatter)
    {
        LogLevel = logLevel;
        EventId = eventId;
        State = state;
        Exception = exception;
        Formatter = formatter;
    }
}