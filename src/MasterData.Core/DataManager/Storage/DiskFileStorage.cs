using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Storage;

public class DiskFileStorage : IFileStorage
{
    public virtual string GetFolderPath(FormElement formElement, FormElementField field, Dictionary<string, object> values)
    {
        if (field.DataFile == null)
            throw new ArgumentException(@$"{nameof(FormElementField.DataFile)} not defined.", field.Name);

        if (string.IsNullOrEmpty(field.DataFile.FolderPath))
            throw new ArgumentException(@$"{nameof(FormElementField.DataFile.FolderPath)} cannot be empty.", field.Name);

        var pkValues = DataHelper.ParsePkValues(formElement, values, '_');
        return CombineKey(ResolveFolderPath(field.DataFile.FolderPath), pkValues);
    }

    public async Task SaveAsync(string fullPath, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(fullPath);
        var folderPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(folderPath))
            Directory.CreateDirectory(folderPath);

        var mode = replaceIfExists ? FileMode.Create : FileMode.CreateNew;

        if (content.CanSeek)
            content.Seek(0, SeekOrigin.Begin);

        await using var fileStream = new FileStream(filePath, mode, FileAccess.Write, FileShare.None, 81920, true);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(fullPath);
        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string fullPath, CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(fullPath);
        if (File.Exists(filePath))
            File.Delete(filePath);
        else
            throw new KeyNotFoundException("File not found");

        return Task.CompletedTask;
    }

    public Task DeleteFolderAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var resolvedFolderPath = ResolveFolderPath(folderPath);
        if (Directory.Exists(resolvedFolderPath))
            Directory.Delete(resolvedFolderPath, true);

        return Task.CompletedTask;
    }

    public Task MoveAsync(string currentFullPath, string newFullPath, CancellationToken cancellationToken = default)
    {
        var currentPath = ResolveFilePath(currentFullPath);
        var newPath = ResolveFilePath(newFullPath);

        if (!File.Exists(currentPath))
            throw new KeyNotFoundException("File not found");

        var newFolderPath = Path.GetDirectoryName(newPath);
        if (!string.IsNullOrEmpty(newFolderPath))
            Directory.CreateDirectory(newFolderPath);

        File.Move(currentPath, newPath, true);

        return Task.CompletedTask;
    }

    public Task<List<FileStorageItem>> ListAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var resolvedFolderPath = ResolveFolderPath(folderPath);
        if (!Directory.Exists(resolvedFolderPath))
            return Task.FromResult(new List<FileStorageItem>());

        var files = new DirectoryInfo(resolvedFolderPath)
            .EnumerateFiles()
            .Select(file => new FileStorageItem
            {
                FileName = file.Name,
                Length = file.Length,
                LastWriteTime = file.LastWriteTime,
                FolderPath = folderPath
            })
            .ToList();

        return Task.FromResult(files);
    }

    protected virtual string ResolveFolderPath(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
            throw new ArgumentNullException(nameof(folderPath));

        var separator = Path.DirectorySeparatorChar;
        var resolvedFolderPath = folderPath.Replace("{app.path}", FileIO.GetApplicationPath().TrimEnd(separator));

        return Path.GetFullPath(resolvedFolderPath);
    }

    private string ResolveFilePath(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            throw new ArgumentNullException(nameof(fullPath));

        var normalizedFullPath = fullPath.Replace('\\', '/');
        var folderPath = Path.GetDirectoryName(normalizedFullPath);
        if (string.IsNullOrEmpty(folderPath))
            throw new ArgumentException("File path must include a folder.", nameof(fullPath));

        return FileStoragePath.GetFilePath(ResolveFolderPath(folderPath), FileStoragePath.GetFileName(normalizedFullPath));
    }

    private static string CombineKey(string rootKey, string childKey)
    {
        return string.IsNullOrEmpty(childKey) ? rootKey : $"{rootKey.TrimEnd('/', '\\')}/{childKey}";
    }
}
