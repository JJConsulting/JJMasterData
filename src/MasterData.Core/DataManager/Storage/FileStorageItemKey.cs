namespace JJMasterData.Core.DataManager.Storage;

public sealed record FileStorageItemKey(string FullPath, bool IsTemporary)
{
    public string FileName => FileStoragePath.GetFileName(FullPath);
}
