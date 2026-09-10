#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Background;

public abstract class BackgroundJobHandler<TRequest> where TRequest : BackgroundJobRequest
{
    public abstract Task<object?> ExecuteAsync(
        TRequest request,
        IProgress<BackgroundJobProgress> progress,
        CancellationToken cancellationToken);
}
