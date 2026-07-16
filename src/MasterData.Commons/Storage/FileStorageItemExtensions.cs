using JJConsulting.MasterData.Abstractions;

namespace JJMasterData.Commons.Storage;

public static class FileStorageItemExtensions
{
    extension(FileStorageItem item)
    {
        public string FullPath
        {
            get
            {
                if (string.IsNullOrEmpty(item.FolderPath))
                    return FileStoragePath.GetFileName(item.FileName);
            
                return FileStoragePath.Combine(item.FolderPath, item.FileName);
            }
        }
    }
}