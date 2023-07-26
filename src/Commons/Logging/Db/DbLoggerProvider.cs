using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias(ProviderName)]
internal class DbLoggerProvider : ILoggerProvider
{
    public const string ProviderName = "Database";

    private readonly ILogger _logger;

    public DbLoggerProvider(DbLoggerBuffer buffer, IOptionsMonitor<DbLoggerOptions> options)
    {
        _logger = new DbLogger(buffer,options);
    }

    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose(){}
}