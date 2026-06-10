using System.IO;

namespace JJMasterData.Core.DataManager.Storage;

public static class FileStoragePath
{
    public static string Combine(string folderPath, string fileName)
    {
        var safeFileName = GetFileName(fileName);

        return string.IsNullOrEmpty(folderPath)
            ? safeFileName
            : $"{folderPath.TrimEnd('/', '\\')}/{safeFileName}";
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileName(path?.Replace('\\', '/') ?? string.Empty);
    }

    public static string GetFilePath(string folderPath, string fileName)
    {
        return Path.Combine(folderPath, GetFileName(fileName));
    }
}
