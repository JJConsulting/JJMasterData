using System;
using JJMasterData.Commons.Dao.Entity.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias("Database")]
public class DbLoggerProvider : ILoggerProvider
{
    internal readonly DbLoggerOptions Options;
    internal readonly IEntityRepository Repository;
 
    public DbLoggerProvider(IOptions<DbLoggerOptions> options, IServiceProvider serviceProvider)
    {
        Options = options.Value;

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