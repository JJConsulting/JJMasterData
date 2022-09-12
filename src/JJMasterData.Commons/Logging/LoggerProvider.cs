using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

public sealed class LoggerProvider : ILoggerProvider
{

    private readonly ConcurrentDictionary<string, Logger> _loggers = new(StringComparer.OrdinalIgnoreCase);


    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new Logger(name));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
