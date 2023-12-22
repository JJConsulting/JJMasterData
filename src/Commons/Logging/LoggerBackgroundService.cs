using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace JJMasterData.Commons.Logging;

public abstract class LoggerBackgroundService<TLoggerBuffer>(TLoggerBuffer loggerBuffer) : BackgroundService
    where TLoggerBuffer : LoggerBuffer
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (loggerBuffer.TryDequeue(out var message))
            {
                await LogAsync(message, cancellationToken);
            }

            await Task.Delay(500, cancellationToken);
        }
    }

    protected abstract Task LogAsync(LogMessage logMessage, CancellationToken cancellationToken);
}
