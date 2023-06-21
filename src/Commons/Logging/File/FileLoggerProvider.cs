using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.File;

[ProviderAlias(ProviderName)]
public class FileLoggerProvider : ILoggerProvider
{
    public const string ProviderName = "File";

    private readonly ILogger _logger;

    public FileLoggerProvider(FileLoggerBuffer buffer)
    {
        _logger = new FileLogger(buffer);
    }
    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose(){}
}