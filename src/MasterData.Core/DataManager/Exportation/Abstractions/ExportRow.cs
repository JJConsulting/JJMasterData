using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Exportation.Abstractions;

public sealed class ExportRow(
    Dictionary<string, object?> values,
    Dictionary<string, string> formattedValues)
{
    public Dictionary<string, object?> Values { get; } = values;
    public Dictionary<string, string> FormattedValues { get; } = formattedValues;
}
