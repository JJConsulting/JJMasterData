using System;
using System.IO;
using JJMasterData.Commons.Util;

namespace JJMasterData.Commons.Storage;

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

    private static string GetFilePath(string folderPath, string fileName)
    {
        return Path.Combine(folderPath, GetFileName(fileName));
    }
    
    public static string ResolveFolderPath(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
            throw new ArgumentNullException(nameof(folderPath));

        var separator = Path.DirectorySeparatorChar;
        var resolvedFolderPath = folderPath.Replace("{app.path}", FileIO.GetApplicationPath().TrimEnd(separator));
        
        return Path.GetFullPath(resolvedFolderPath);
    }

    public static string ResolveFullPath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            throw new ArgumentNullException(nameof(fullPath));

        var normalizedFullPath = fullPath.Replace('\\', '/');
        var folderPath = Path.GetDirectoryName(normalizedFullPath);
        if (string.IsNullOrEmpty(folderPath))
            throw new ArgumentException(@"File path must include a folder.", nameof(fullPath));

        var resolvedFolderPath = ResolveFolderPath(folderPath);
        
        return GetFilePath(resolvedFolderPath, GetFileName(normalizedFullPath));
    }
}
