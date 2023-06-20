using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace JJMasterData.Commons.Logging;

public abstract class LoggerBackgroundService<TLoggerBuffer> : BackgroundService where TLoggerBuffer : LoggerBuffer
{
    private readonly TLoggerBuffer loggerBuffer;

    protected LoggerBackgroundService(TLoggerBuffer loggerBuffer)
    {
        this.loggerBuffer = loggerBuffer;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (loggerBuffer.TryDequeue(out var message))
            {
                await LogAsync(message, cancellationToken);
            }

            await Task.Delay(100, cancellationToken);
        }
    }

    protected abstract Task LogAsync(LogMessage logMessage, CancellationToken cancellationToken);
}
