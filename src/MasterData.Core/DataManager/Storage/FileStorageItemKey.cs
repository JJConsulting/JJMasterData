namespace JJMasterData.Core.DataManager.Storage;

public sealed record FileStorageItemKey(string FolderPath, string FileName, bool IsTemporary);