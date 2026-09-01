using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Background;

namespace JJMasterData.Core.DataManager.Importation.Background;

public sealed class ImportJobService(IBackgroundJobClient jobs, ImportFormatCatalog formats)
{
    public ValueTask<Guid> EnqueueAsync(ImportRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ElementName);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.UserId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.FilePath);
        formats.Resolve(request.FormatId, request.FileName, request.ContentType).ValidateOptions(request.FormatOptions);
        cancellationToken.ThrowIfCancellationRequested();
        var snapshot = new ImportRequest
        {
            ElementName = request.ElementName,
            UserId = request.UserId,
            FilePath = request.FilePath,
            FileName = request.FileName,
            ContentType = request.ContentType,
            FormatId = request.FormatId,
            FormatOptions = new Dictionary<string, string?>(request.FormatOptions, StringComparer.OrdinalIgnoreCase),
            RelationValues = new Dictionary<string, object?>(request.RelationValues, StringComparer.OrdinalIgnoreCase),
            UserValues = new Dictionary<string, object?>(request.UserValues, StringComparer.OrdinalIgnoreCase),
            IpAddress = request.IpAddress,
            BrowserInfo = request.BrowserInfo,
            CommandBeforeProcess = request.CommandBeforeProcess,
            CommandAfterProcess = request.CommandAfterProcess
        };
        return jobs.EnqueueAsync(snapshot, cancellationToken);
    }

    public ImportJobStatus? GetStatus(Guid id, string userId)
    {
        var status = jobs.GetStatus(id, userId);
        return status is null ? null : new ImportJobStatus
        {
            Id = status.Id,
            State = status.State,
            Progress = status.Progress,
            Result = status.Result as ImportJobResult,
            Error = status.Error,
            CreatedAt = status.CreatedAt,
            StartedAt = status.StartedAt,
            CompletedAt = status.CompletedAt
        };
    }

    public bool Cancel(Guid id, string userId) => jobs.Cancel(id, userId);
}