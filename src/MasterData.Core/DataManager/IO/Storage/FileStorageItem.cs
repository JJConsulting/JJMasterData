using System;

namespace JJMasterData.Core.DataManager.IO.Storage;

public sealed class FileStorageItem
{
    public string FileName { get; init; }
    public long Length { get; init; }
    public DateTime LastWriteTime { get; init; }
}
