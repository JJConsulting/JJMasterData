#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Background.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Background;

internal sealed class BackgroundJobService(
    BackgroundJobQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundJobService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumers = new List<Task>(queue.MaxConcurrency);
        for (var i = 0; i < queue.MaxConcurrency; i++)
            consumers.Add(ConsumeAsync(stoppingToken));

        try
        {
            await Task.WhenAll(consumers);
        }
        finally
        {
            queue.CancelQueuedJobs();
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        queue.Complete();
        await base.StopAsync(cancellationToken);
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in queue.Reader.ReadAllAsync(stoppingToken))
        {
            if (!queue.TryGetEntry(workItem.Id, out var entry) || !entry.TryStart())
                continue;

            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken, entry.CancellationToken);

            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var result = await workItem.ExecuteAsync(
                    scope.ServiceProvider,
                    new Progress<BackgroundJobProgress>(entry.Report),
                    linkedCancellation.Token);
                entry.Succeed(result);
            }
            catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
            {
                entry.MarkCancelled();
            }
            catch (Exception exception)
            {
                entry.Fail(exception);
                logger.LogError(exception, "Background job {JobId} failed", workItem.Id);
            }
        }
    }
}
