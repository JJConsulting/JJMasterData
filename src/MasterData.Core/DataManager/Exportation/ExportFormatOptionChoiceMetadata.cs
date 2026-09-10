namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportFormatOptionChoiceMetadata(string value, string displayName)
{
    public string Value { get; } = value;
    public string DisplayName { get; } = displayName;
}