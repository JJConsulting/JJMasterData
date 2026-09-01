#nullable enable
using System;

namespace JJMasterData.Commons.Background;

public sealed class BackgroundJobSnapshot
{
    public required Guid Id { get; init; }
    public required string UserId { get; init; }
    public required BackgroundJobState State { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset? StartedAt { get; init; }
    public required DateTimeOffset? CompletedAt { get; init; }
    public required BackgroundJobProgress? Progress { get; init; }
    public required object? Result { get; init; }
    public required string? Error { get; init; }
}
