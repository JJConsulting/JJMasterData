using System.Collections.Generic;
using JJMasterData.Core.DataManager.Exportation.Abstractions;

namespace JJMasterData.Core.DataManager.Exportation;

public sealed class ExportFormatMetadata
{
    public required IExportFormat Format { get; init; } 
    public required IReadOnlyList<ExportFormatOptionMetadata> Options { get; init; } 
}