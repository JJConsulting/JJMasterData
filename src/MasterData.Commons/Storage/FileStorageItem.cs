using System;

namespace JJMasterData.Commons.Storage;

public sealed class FileStorageItem
{
    public required string FolderPath { get; init; }
    
    public required string FileName { get; init; }
    
    public required long Length { get; init; }
    
    public required DateTime LastWriteTime { get; init; }
    
    public string FullPath
    {
        get
        {
            if (string.IsNullOrEmpty(FolderPath))
                return FileStoragePath.GetFileName(FileName);
            
            return FileStoragePath.Combine(FolderPath, FileName);
        }
    }
}
