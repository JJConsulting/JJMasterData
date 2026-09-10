using System;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Background;

namespace JJMasterData.Core.DataManager.Exportation.Background;

public sealed class ExportJobService(IBackgroundJobClient jobs)
{
    private const string OperationName = "export";

    public ValueTask<Guid> EnqueueAsync(
        ExportRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return jobs.EnqueueAsync(request, cancellationToken);
    }

    public BackgroundJobSnapshot? GetStatus(Guid id, string userId) => jobs.GetStatus(id, userId);

    public BackgroundJobSnapshot? GetCurrentStatus(string elementName, string userId)
    {
        var status = GetStatus(BackgroundJobId.Create(OperationName, elementName, userId), userId);
        return status?.State is BackgroundJobState.Queued or BackgroundJobState.Running ? status : null;
    }

    public bool Cancel(Guid id, string userId) => jobs.Cancel(id, userId);

}
