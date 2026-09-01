namespace JJMasterData.Core.DataManager.Exportation.Background;

public sealed class ExportJobResult
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required string StoragePath { get; init; }
    public required long TotalRecords { get; init; }
}