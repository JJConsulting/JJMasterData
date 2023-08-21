using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.Db;

[ProviderAlias(ProviderName)]
internal class DbLoggerProvider : ILoggerProvider
{
    public const string ProviderName = "Database";

    private readonly ILogger _logger;

    public DbLoggerProvider(DbLoggerBuffer buffer)
    {
        _logger = new DbLogger(buffer);
    }

    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose(){}
}