using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias(ProviderName)]
internal class DbLoggerProvider(DbLoggerBuffer buffer) : ILoggerProvider
{
    private const string ProviderName = "Database";

    private readonly ConcurrentDictionary<string, DbLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new DbLogger(name, buffer));

    public void Dispose()
    {
        _loggers.Clear();
    }
}