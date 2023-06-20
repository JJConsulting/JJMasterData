using System;
using System.Collections.Concurrent;
using JJMasterData.Commons.Data.Entity.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias("Database")]
public class DbLoggerProvider : ILoggerProvider
{
    private IEntityRepository EntityRepository { get; }
    private DbLoggerOptions Options { get; set; }
    
    private readonly BlockingCollection<LogMessage> _queue;
    private readonly ConcurrentDictionary<string, DbLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);
    
    private readonly IDisposable _onChangeOptions;
    public DbLoggerProvider(IOptionsMonitor<DbLoggerOptions> options, IEntityRepository entityRepository)
    {
        EntityRepository = entityRepository;
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
        _loggers.GetOrAdd(categoryName, name => new DbLogger(name, _queue,EntityRepository,GetCurrentOptions));

    private DbLoggerOptions GetCurrentOptions() => Options;

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeOptions.Dispose();
    }
}