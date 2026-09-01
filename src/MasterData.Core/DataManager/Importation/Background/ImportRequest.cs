using System.Collections.Generic;
using JJMasterData.Commons.Background;

namespace JJMasterData.Core.DataManager.Importation.Background;

public sealed class ImportRequest : BackgroundJobRequest
{
    public required string ElementName { get; init; }
    public override required string UserId { get; init; }
    public required string FilePath { get; init; }
    public required string FileName { get; init; }
    public string? ContentType { get; init; }
    public string? FormatId { get; init; }
    public required Dictionary<string, string?> FormatOptions { get; init; }
    public required Dictionary<string, object?> RelationValues { get; init; }
    public required Dictionary<string, object?> UserValues { get; init; }
    public string? IpAddress { get; init; }
    public string? BrowserInfo { get; init; }
    public string? CommandBeforeProcess { get; init; }
    public string? CommandAfterProcess { get; init; }
}