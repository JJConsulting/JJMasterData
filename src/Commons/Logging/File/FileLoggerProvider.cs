using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging.File;

[ProviderAlias("File")]
public class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerBuffer _buffer;
    public FileLoggerProvider(FileLoggerBuffer buffer)
    {
        _buffer = buffer;
    }
    public ILogger CreateLogger(string categoryName) => new FileLogger(_buffer);
    public void Dispose(){}
}