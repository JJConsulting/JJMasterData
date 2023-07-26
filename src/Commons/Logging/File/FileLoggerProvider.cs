using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.File;

[ProviderAlias(ProviderName)]
public class FileLoggerProvider : ILoggerProvider
{
    public const string ProviderName = "File";

    private readonly ILogger _logger;

    public FileLoggerProvider(FileLoggerBuffer buffer, IOptionsMonitor<FileLoggerOptions> options)
    {
        _logger = new FileLogger(buffer,options);
    }
    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose()
    {
        
    }
}