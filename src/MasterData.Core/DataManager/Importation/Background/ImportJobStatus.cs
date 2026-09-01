using System;
using JJMasterData.Commons.Background;

namespace JJMasterData.Core.DataManager.Importation.Background;

public sealed class ImportJobStatus
{
    public required Guid Id { get; init; }
    public required BackgroundJobState State { get; init; }
    public required BackgroundJobProgress? Progress { get; init; }
    public required ImportJobResult? Result { get; init; }
    public required string? Error { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset? StartedAt { get; init; }
    public required DateTimeOffset? CompletedAt { get; init; }
}