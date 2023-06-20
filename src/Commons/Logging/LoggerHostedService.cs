using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace JJMasterData.Commons.Logging;

public abstract class LoggerHostedService<TLoggerBuffer> : IHostedService, IDisposable where TLoggerBuffer : LoggerBuffer
{
    private readonly TLoggerBuffer loggerBuffer;
    private readonly CancellationTokenSource cancellationTokenSource;
    private Task loggingTask;

    protected LoggerHostedService(TLoggerBuffer loggerBuffer)
    {
        this.loggerBuffer= loggerBuffer;
        cancellationTokenSource = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        loggingTask = Task.Factory.StartNew(() => ProcessLogsAsync(cancellationTokenSource.Token),cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationTokenSource.Cancel();
        return loggingTask;
    }

    private async Task ProcessLogsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (loggerBuffer.TryDequeue(out var message))
            {
                Log(message);
            }
            await Task.Delay(1000, cancellationToken);
        }
    }

    protected abstract void Log(LogMessage logMessage);

    public void Dispose()
    {
        cancellationTokenSource.Dispose();
    }
}
