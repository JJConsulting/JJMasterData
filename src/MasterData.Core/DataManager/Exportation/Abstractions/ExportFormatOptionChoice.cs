namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public sealed class ExportFormatOptionChoice(string value, string displayName)
{
    public string Value { get; } = value;
    public string DisplayName { get; } = displayName;
}