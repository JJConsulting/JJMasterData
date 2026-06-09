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

    public async Task SaveAsync(string folderPath, string fileName, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default)
    {
        var resolvedFolderPath = ResolveFolderPath(folderPath);
        Directory.CreateDirectory(resolvedFolderPath);

        var filePath = GetFilePath(folderPath, fileName);
        var mode = replaceIfExists ? FileMode.Create : FileMode.CreateNew;

        if (content.CanSeek)
            content.Seek(0, SeekOrigin.Begin);

        await using var fileStream = new FileStream(filePath, mode, FileAccess.Write, FileShare.None, 81920, true);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string folderPath, string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(ResolveFolderPath(folderPath), fileName);
        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string folderPath, string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(ResolveFolderPath(folderPath), fileName);
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

    public Task RenameAsync(string folderPath, string currentName, string newName, CancellationToken cancellationToken = default)
    {
        var resolvedFolderPath = ResolveFolderPath(folderPath);
        var currentPath = GetFilePath(resolvedFolderPath, currentName);
        var newPath = GetFilePath(resolvedFolderPath, newName);

        if (!File.Exists(currentPath))
            throw new KeyNotFoundException("File not found");

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
                LastWriteTime = file.LastWriteTime
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

    protected static string CombineKey(string rootKey, string childKey)
    {
        return string.IsNullOrEmpty(childKey) ? rootKey : $"{rootKey.TrimEnd('/', '\\')}/{childKey}";
    }

    private static string GetFilePath(string folderPath, string fileName)
    {
        fileName = Path.GetFileName(fileName);
        return Path.Combine(folderPath, fileName);
    }
}
