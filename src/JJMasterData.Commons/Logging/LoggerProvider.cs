using System;
using System.Collections.Concurrent;
using JJMasterData.Commons.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging;

public sealed class LoggerProvider : ILoggerProvider
{

    private readonly ConcurrentDictionary<string, Logger> _loggers = new(StringComparer.OrdinalIgnoreCase);

    private readonly IOptionsMonitor<JJMasterDataOptions> _options;
    public LoggerProvider(IOptionsMonitor<JJMasterDataOptions> options)
    {
        _options = options;
    }
    
    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new Logger(name, _options.CurrentValue.Logger));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}
