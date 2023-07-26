#nullable enable

using System;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

public class LogEntry<TState>
{
    public LogLevel LogLevel { get; }
    public EventId EventId { get; }
    public TState State { get; }
    public Exception? Exception { get; }
    public Func<TState, Exception?, string> Formatter { get; }

    public LogEntry(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogLevel = logLevel;
        EventId = eventId;
        State = state;
        Exception = exception;
        Formatter = formatter;
    }
}