using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Importation;

public sealed class ImportRecord(long rowNumber, List<string?> values)
{
    public long RowNumber { get; init; } = rowNumber;
    public List<string?> Values { get; init; } = values;
}
