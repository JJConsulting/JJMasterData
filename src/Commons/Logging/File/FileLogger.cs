using System;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.File;

public class FileLogger : ILogger
{
    private readonly FileLoggerBuffer _buffer;

    /// <summary>
    /// Creates a new instance of <see cref="FileLogger" />.
    /// </summary>
    public FileLogger(FileLoggerBuffer buffer)
    {
        _buffer = buffer;
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
        _buffer.Enqueue(message);
    }
}