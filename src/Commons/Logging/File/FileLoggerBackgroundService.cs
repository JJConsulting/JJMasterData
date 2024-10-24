using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Logging.File;

public class FileLoggerBackgroundService(
    FileLoggerBuffer loggerBuffer,
    IOptionsMonitor<FileLoggerOptions> optionsMonitor)
    : LoggerBackgroundService<FileLoggerBuffer>(loggerBuffer)
{
    protected override async Task LogAsync(LogMessage logMessage, CancellationToken cancellationToken)
    {
        var options = optionsMonitor.CurrentValue;
        var path = FileIO.ResolveFilePath(options.FileName);
        var directory = Path.GetDirectoryName(path);

        Directory.CreateDirectory(directory!);

#if NET
        await using var writer = new StreamWriter(path, true);
#else
        using var writer = new StreamWriter(path, true);
#endif
        await writer.WriteAsync(logMessage.Message);
    }
}
