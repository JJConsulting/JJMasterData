#nullable enable
using System;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.Db;

public class DbLogger : ILogger
{
    private readonly DbLoggerBuffer _loggerBuffer;
    
    public DbLogger(DbLoggerBuffer loggerBuffer)
    {
        _loggerBuffer = loggerBuffer;
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
        
        var message = new LogMessage(logLevel, eventId, state!, exception, (s, e) => formatter((TState)s, e));
        _loggerBuffer.Enqueue(message);
    }
}