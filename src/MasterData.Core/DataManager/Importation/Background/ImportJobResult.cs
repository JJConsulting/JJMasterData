using System.Collections.Generic;

namespace JJMasterData.Core.DataManager.Importation.Background;

public sealed class ImportJobResult
{
    public required long TotalProcessed { get; init; }
    public required int Inserted { get; init; }
    public required int Updated { get; init; }
    public required int Deleted { get; init; }
    public required int Ignored { get; init; }
    public required int Errors { get; init; }
    public required List<string> ErrorMessages { get; init; }
}