using System.IO;

namespace JJMasterData.Core.DataManager.Storage;

public sealed class FileStorageReference
{
    public string Key { get; init; } = string.Empty;
    public bool IsTemporary { get; init; }

    public string FolderKey
    {
        get
        {
            var index = Key?.LastIndexOf('/') ?? -1;
            return index < 0 ? string.Empty : Key[..index];
        }
    }

    public string FileName => GetSafeFileName(Key);

    public static FileStorageReference Create(string folderKey, string fileName, bool isTemporary)
    {
        var normalizedFolderKey = folderKey?.TrimEnd('/', '\\').Replace('\\', '/');
        var safeFileName = GetSafeFileName(fileName);
        var key = string.IsNullOrEmpty(normalizedFolderKey)
            ? safeFileName
            : $"{normalizedFolderKey}/{safeFileName}";

        return new FileStorageReference
        {
            Key = key,
            IsTemporary = isTemporary
        };
    }

    private static string GetSafeFileName(string value)
    {
        return Path.GetFileName(value?.Replace('\\', '/') ?? string.Empty);
    }
}
