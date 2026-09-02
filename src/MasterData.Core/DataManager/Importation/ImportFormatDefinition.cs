using System.Collections.Generic;
using JJMasterData.Core.DataManager.Exportation;

namespace JJMasterData.Core.DataManager.Importation;

public sealed class ImportFormatDefinition
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required List<string> FileExtensions { get; init; }
    public required List<string> ContentTypes { get; init; }
    public List<ExportFormatOption>? Options { get; init; }
}
