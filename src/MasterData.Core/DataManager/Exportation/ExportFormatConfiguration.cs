using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportFormatConfiguration
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required string FileExtension { get; init; }
    public required string ContentType { get; init; }
    public List<ExportFormatOption>? Options { get; init; }
}
