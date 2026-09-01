#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Background;

public interface IBackgroundJobClient
{
    ValueTask<Guid> EnqueueAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default) where TRequest : BackgroundJobRequest;

    BackgroundJobSnapshot? GetStatus(Guid id, string userId);
    bool Cancel(Guid id, string userId);
}
