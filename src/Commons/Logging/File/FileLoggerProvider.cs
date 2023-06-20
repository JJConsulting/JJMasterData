using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.File;

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    private FileLoggerOptions Options { get; set; }
    private readonly BlockingCollection<LogMessage> _queue;
    private readonly ConcurrentDictionary<string, FileLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);
    
    private readonly IDisposable _onChangeOptions;
    public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
    {
        _queue = new BlockingCollection<LogMessage>();
        _onChangeOptions = options.OnChange(updatedConfig => Options = updatedConfig);
        
        Options = options.CurrentValue;
    }

    /// <summary>
    /// Creates a new instance of the file logger.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _queue,GetCurrentOptions));

    private FileLoggerOptions GetCurrentOptions() => Options;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeOptions.Dispose();
    }
}