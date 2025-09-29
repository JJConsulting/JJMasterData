using System;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Commons.Logging;

[Obsolete("Please use Serilog sinks.")]
public class LogMessage
{
    public required DateTimeOffset Timestamp { get; init; }
    public required LogLevel LogLevel { get; init; }
    public required string Category { get; init; }
    public required string Event { get; init; }
    public required string Message { get; init; }
}