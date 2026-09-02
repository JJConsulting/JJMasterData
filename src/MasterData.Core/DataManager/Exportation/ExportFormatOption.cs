using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportFormatOption
{
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required ExportFormatOptionKind Kind { get; init; }
    public string? DefaultValue { get; init; }
    public List<ExportFormatOptionChoice>? Choices { get; init; }
}
