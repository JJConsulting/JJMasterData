using JJMasterData.Commons.Dao;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias("Database")]
public class DbLoggerProvider : ILoggerProvider
{
    internal readonly DbLoggerOptions Options;
    internal readonly IEntityRepository Repository;
 
    public DbLoggerProvider(IOptions<DbLoggerOptions> options, IEntityRepository _entityRepository)
    {
        Options = options.Value;
        Repository = _entityRepository;
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
 
    public void Dispose()
    {
    }
}