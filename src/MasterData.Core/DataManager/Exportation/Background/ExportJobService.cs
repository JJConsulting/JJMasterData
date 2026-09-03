using System;
using System.Collections.Generic;
using System.Linq;
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
        var snapshot = Snapshot(request);
        return jobs.EnqueueAsync(snapshot, cancellationToken);
    }

    public BackgroundJobSnapshot? GetStatus(Guid id, string userId) => jobs.GetStatus(id, userId);

    public BackgroundJobSnapshot? GetCurrentStatus(string elementName, string userId)
    {
        var status = GetStatus(BackgroundJobId.Create(OperationName, elementName, userId), userId);
        return status?.State is BackgroundJobState.Queued or BackgroundJobState.Running ? status : null;
    }

    public bool Cancel(Guid id, string userId) => jobs.Cancel(id, userId);

    private static ExportRequest Snapshot(ExportRequest request)
    {
        var rows = request.Rows?.Select(row =>
                new Dictionary<string, object?>(row, StringComparer.OrdinalIgnoreCase))
            .ToList();
        return new ExportRequest
        {
            Id = BackgroundJobId.Create(OperationName, request.ElementName, request.UserId),
            ElementName = request.ElementName,
            UserId = request.UserId,
            FormatId = request.FormatId,
            IncludeHeader = request.IncludeHeader,
            ExportAllFields = request.ExportAllFields,
            FormatOptions = new Dictionary<string, string?>(request.FormatOptions, StringComparer.OrdinalIgnoreCase),
            Filters = new Dictionary<string, object?>(request.Filters, StringComparer.OrdinalIgnoreCase),
            OrderBy = request.OrderBy,
            UserValues = new Dictionary<string, object?>(request.UserValues, StringComparer.OrdinalIgnoreCase),
            Rows = rows
        };
    }

}
