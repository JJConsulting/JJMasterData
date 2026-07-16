namespace JJConsulting.MasterData.Abstractions;

public sealed class FileStorageItem
{
    public required string FolderPath { get; init; }
    
    public required string FileName { get; init; }
    
    public required long Length { get; init; }
    
    public required DateTime LastWriteTime { get; init; }
}
