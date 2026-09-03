using System.Collections.Generic;

namespace JJMasterData.Core.DataManager;

public enum FormatOptionKind
{
    Input,
    Boolean,
    Select
}

public sealed class FormatOptionChoiceMetadata(string value, string displayName)
{
    public string Value { get; } = value;
    public string DisplayName { get; } = displayName;
}

public sealed class FormatOptionMetadata(
    string name,
    string displayName,
    FormatOptionKind kind,
    string? defaultValue,
    IReadOnlyList<FormatOptionChoiceMetadata> choices)
{
    public string Name { get; } = name;
    public string DisplayName { get; } = displayName;
    public FormatOptionKind Kind { get; } = kind;
    public string? DefaultValue { get; } = defaultValue;
    public IReadOnlyList<FormatOptionChoiceMetadata> Choices { get; } = choices;
}
