#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Commons.Background.Queue;

// Essa interface existe para os channels que usam ela não ficarem com <T>
internal interface IBackgroundJobWorkItem
{
    Guid Id { get; }

    Task<object?> ExecuteAsync(
        IServiceProvider serviceProvider,
        IProgress<BackgroundJobProgress> progress,
        CancellationToken cancellationToken);
}

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