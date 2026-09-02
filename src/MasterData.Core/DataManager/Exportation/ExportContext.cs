using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportContext
{
    public required FormElement FormElement { get; init; }
    public required List<ExportColumn> Columns { get; init; }
    public required IAsyncEnumerable<Dictionary<string, object?>> Rows { get; init; }
    public required Dictionary<string, object?> UserValues { get; init; }
    public required bool IncludeHeader { get; init; }
    public required long? TotalRecords { get; init; }
    public required IProgress<ExportProgress> Progress { get; init; }
}
