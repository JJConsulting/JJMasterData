#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Background.Queue;

internal interface IBackgroundJobWorkItem
{
    Guid Id { get; }

    Task<object?> ExecuteAsync(
        IServiceProvider serviceProvider,
        IProgress<BackgroundJobProgress> progress,
        CancellationToken cancellationToken);
}