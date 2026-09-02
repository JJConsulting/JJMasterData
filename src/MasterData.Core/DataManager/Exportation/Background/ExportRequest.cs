using System.Collections.Generic;
using JJMasterData.Commons.Background;

namespace JJMasterData.Core.DataManager.Exportation.Background;

public sealed class ExportRequest : BackgroundJobRequest
{
    public required string ElementName { get; init; }
    public override required string UserId { get; init; }
    public required string FormatId { get; init; }
    public required bool IncludeHeader { get; init; }
    public required bool ExportAllFields { get; init; }
    public required Dictionary<string, string?> FormatOptions { get; init; }
    public required Dictionary<string, object?> Filters { get; init; }
    public string? OrderBy { get; init; }
    public required Dictionary<string, object?> UserValues { get; init; }
    public List<Dictionary<string, object?>>? Rows { get; init; }
}
