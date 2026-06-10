using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Storage;

public class DiskFileStorage : IFileStorage
{
    public async Task SaveAsync(string fullPath, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default)
    {
        var filePath = FileStoragePath.ResolveFullPath(fullPath);
        
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
        var filePath = FileStoragePath.ResolveFullPath(fullPath);
        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string fullPath, CancellationToken cancellationToken = default)
    {
        var filePath = FileStoragePath.ResolveFullPath(fullPath);
        if (File.Exists(filePath))
            File.Delete(filePath);
        else
            throw new KeyNotFoundException("File not found");

        return Task.CompletedTask;
    }

    public Task DeleteFolderAsync(string folderPath, CancellationToken cancellationToken = default)
    {
        var resolvedFolderPath = FileStoragePath.ResolveFolderPath(folderPath);
        if (Directory.Exists(resolvedFolderPath))
            Directory.Delete(resolvedFolderPath, true);

        return Task.CompletedTask;
    }

    public Task MoveAsync(string currentFullPath, string newFullPath, CancellationToken cancellationToken = default)
    {
        var currentPath = FileStoragePath.ResolveFullPath(currentFullPath);
        var newPath = FileStoragePath.ResolveFullPath(newFullPath);

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
        var resolvedFolderPath = FileStoragePath.ResolveFolderPath(folderPath);
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
}
