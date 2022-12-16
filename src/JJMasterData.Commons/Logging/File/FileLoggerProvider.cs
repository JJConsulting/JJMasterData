using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.File;

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    internal readonly FileLoggerOptions Options;

    public FileLoggerProvider(IOptions<FileLoggerOptions> options)
    {
        Options = options.Value;
    }
 
    /// <summary>
    /// Creates a new instance of the file logger.
    /// </summary>
    /// <param name="categoryName"></param>
    /// <returns></returns>
    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(this);
    }

    public void Dispose(){}
}