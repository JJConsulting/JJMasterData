using System;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Background;

namespace JJMasterData.Core.DataManager.Importation.Background;

public sealed class ImportJobService(IBackgroundJobClient jobs)
{
    private const string OperationName = "import";

    public ValueTask<Guid> EnqueueAsync(ImportRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ElementName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FilePath);

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
