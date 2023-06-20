using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.File;

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    private readonly ILogger _logger;

    public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
    {
        _logger = new FileLogger(options);
    }
 
    /// <summary>
    /// Creates a new instance of the file logger.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose(){}
}