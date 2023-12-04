using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.File;

public class FileLoggerBackgroundService : LoggerBackgroundService<FileLoggerBuffer>
{
    private readonly IOptionsMonitor<FileLoggerOptions> _optionsMonitor;

    public FileLoggerBackgroundService(FileLoggerBuffer loggerBuffer, IOptionsMonitor<FileLoggerOptions> optionsMonitor) : base(loggerBuffer)
    {
        _optionsMonitor = optionsMonitor;
    }
    
    protected override async Task LogAsync(char[] logMessage, CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.CurrentValue;
        var path = FileIO.ResolveFilePath(options.FileName);
        var directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory!);

#if NET
        await using var writer = new StreamWriter(path, true);
#else
        using var writer = new StreamWriter(path, true);
#endif
        await writer.WriteAsync(logMessage);
    }
}
