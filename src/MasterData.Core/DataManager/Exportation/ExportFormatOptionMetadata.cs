using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportFormatOptionMetadata(
    string name,
    string displayName,
    ExportFormatOptionKind kind,
    string? defaultValue,
    IReadOnlyList<ExportFormatOptionChoiceMetadata> choices)
{
    public string Name { get; } = name;
    public string DisplayName { get; } = displayName;
    public ExportFormatOptionKind Kind { get; } = kind;
    public string? DefaultValue { get; } = defaultValue;
    public IReadOnlyList<ExportFormatOptionChoiceMetadata> Choices { get; } = choices;
}
