#nullable enable
using System;

namespace JJMasterData.Commons.Logging;

public struct LogMessage
{
    public required DateTime Created { get; init; }
    public required int LogLevel { get; init; }
    public required string Category { get; init; }
    public required string Event { get; init; }
    public required string Message { get; init; }
}