using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.IO.Storage;

public class DiskFileStorage : IFileStorage
{
    public virtual string GetFolderKey(FormElement formElement, FormElementField field, Dictionary<string, object> values)
    {
        if (field.DataFile == null)
            throw new ArgumentException(@$"{nameof(FormElementField.DataFile)} not defined.", field.Name);

        if (string.IsNullOrEmpty(field.DataFile.FolderPath))
            throw new ArgumentException(@$"{nameof(FormElementField.DataFile.FolderPath)} cannot be empty.", field.Name);

        var pkValues = DataHelper.ParsePkValues(formElement, values, '_');
        return CombineKey(ResolveFolderPath(field.DataFile.FolderPath), pkValues);
    }

    public async Task SaveAsync(string folderKey, string fileName, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default)
    {
        var folderPath = ResolveFolderPath(folderKey);
        Directory.CreateDirectory(folderPath);

        var filePath = GetFilePath(folderPath, fileName);
        var mode = replaceIfExists ? FileMode.Create : FileMode.CreateNew;

        if (content.CanSeek)
            content.Seek(0, SeekOrigin.Begin);

        await using var fileStream = new FileStream(filePath, mode, FileAccess.Write, FileShare.None, 81920, true);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(string folderKey, string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(ResolveFolderPath(folderKey), fileName);
        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string folderKey, string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(ResolveFolderPath(folderKey), fileName);
        if (File.Exists(filePath))
            File.Delete(filePath);
        else
            throw new KeyNotFoundException("File not found");

        return Task.CompletedTask;
    }

    public Task DeleteFolderAsync(string folderKey, CancellationToken cancellationToken = default)
    {
        var folderPath = ResolveFolderPath(folderKey);
        if (Directory.Exists(folderPath))
            Directory.Delete(folderPath, true);

        return Task.CompletedTask;
    }

    public Task RenameAsync(string folderKey, string currentName, string newName, CancellationToken cancellationToken = default)
    {
        var folderPath = ResolveFolderPath(folderKey);
        var currentPath = GetFilePath(folderPath, currentName);
        var newPath = GetFilePath(folderPath, newName);

        if (!File.Exists(currentPath))
            throw new KeyNotFoundException("File not found");

        File.Move(currentPath, newPath, true);
        return Task.CompletedTask;
    }

    public Task<List<FileStorageItem>> ListAsync(string folderKey, CancellationToken cancellationToken = default)
    {
        var folderPath = ResolveFolderPath(folderKey);
        if (!Directory.Exists(folderPath))
            return Task.FromResult(new List<FileStorageItem>());

        var files = new DirectoryInfo(folderPath)
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

    protected virtual string ResolveFolderPath(string folderKey)
    {
        if (string.IsNullOrEmpty(folderKey))
            throw new ArgumentNullException(nameof(folderKey));

        var separator = Path.DirectorySeparatorChar;
        var folderPath = folderKey.Replace("{app.path}", FileIO.GetApplicationPath().TrimEnd(separator));

        return Path.GetFullPath(folderPath);
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
