using System;
using System.Collections.Concurrent;
using JJMasterData.Commons.Data.Entity.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias("Database")]
public class DbLoggerProvider : ILoggerProvider
{
    private readonly DbLoggerBuffer _buffer;
    public DbLoggerProvider(DbLoggerBuffer buffer)
    {
        _buffer = buffer;
    }
    public ILogger CreateLogger(string categoryName) => new DbLogger(_buffer);
    public void Dispose(){}
}