#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Background.Queue;

internal sealed class BackgroundJobWorkItem<TRequest>(Guid id, TRequest request) : IBackgroundJobWorkItem
    where TRequest : BackgroundJobRequest
{
    public Guid Id { get; } = id;

    public Task<object?> ExecuteAsync(
        IServiceProvider serviceProvider,
        IProgress<BackgroundJobProgress> progress,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<BackgroundJobHandler<TRequest>>();
        return handler.ExecuteAsync(request, progress, cancellationToken);
    }
}