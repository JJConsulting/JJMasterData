using System;
using JJMasterData.Commons.Data.Entity.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias("Database")]
public class DbLoggerProvider : ILoggerProvider
{
    internal IOptionsMonitor<DbLoggerOptions> Options { get; }
    internal IEntityRepository Repository { get; }
    internal bool TableExists { get; set; }
 
    public DbLoggerProvider(IOptionsMonitor<DbLoggerOptions> options, IServiceProvider serviceProvider)
    {
        Options = options;

        using var scope = serviceProvider.CreateScope();
        Repository = scope.ServiceProvider.GetRequiredService<IEntityRepository>();
    }
 
    /// <summary>
    /// Creates a new instance of the db logger.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new DbLogger(this);
    }
 
    public void Dispose(){}
}