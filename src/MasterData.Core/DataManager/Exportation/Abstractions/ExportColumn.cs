using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public sealed class ExportColumn(string name, string displayName, FormElementField field)
{
    public string Name { get; } = name;
    public string DisplayName { get; } = displayName;
    public FormElementField Field { get; } = field;
}