using System;
using System.IO;

namespace JJMasterData.Core.DataManager.Storage;

public sealed class FileStorageItem
{
    public required string FolderPath { get; init; }
    public required string FileName { get; init; }
    public bool IsTemporary { get; set; }
    
    public required long Length { get; init; }
    
    public required DateTime LastWriteTime { get; init; }
    
    public string FullPath
    {
        get
        {
            var safeFileName = GetSafeFileName(FileName);
            
            if (string.IsNullOrEmpty(FolderPath))
                return safeFileName;
            
            return $"{FolderPath}/{safeFileName}";
        }
    }

    private static string GetSafeFileName(string value)
    {
        return Path.GetFileName(value?.Replace('\\', '/') ?? string.Empty);
    }
}