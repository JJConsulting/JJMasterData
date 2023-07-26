using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace JJMasterData.Commons.Logging;

public abstract class LoggerBackgroundService<TLoggerBuffer> : BackgroundService where TLoggerBuffer : LoggerBuffer
{
    private readonly TLoggerBuffer _loggerBuffer;

    protected LoggerBackgroundService(TLoggerBuffer loggerBuffer)
    {
        _loggerBuffer = loggerBuffer;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_loggerBuffer.TryDequeue(out var message))
            {
                await LogAsync(message, cancellationToken);
            }

            await Task.Delay(500, cancellationToken);
        }
    }

    protected abstract Task LogAsync(char[] logMessage, CancellationToken cancellationToken);
}
