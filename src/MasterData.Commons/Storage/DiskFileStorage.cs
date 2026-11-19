using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JJConsulting.MasterData.Storage.Abstractions;

namespace JJMasterData.Commons.Storage;

internal sealed class DiskFileStorage : IFileStorage
{
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
        var resolvedFolderPath = FileStoragePath.ResolveFolderPath(folderPath);
        RejectReparsePoint(resolvedFolderPath);
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

    public Task<List<FileStorageItem>> ListAsync(string folderPath, bool isRecursive = false, CancellationToken cancellationToken = default)
    {
        var resolvedFolderPath = FileStoragePath.ResolveFolderPath(folderPath);
        RejectReparsePoint(resolvedFolderPath);
        if (!Directory.Exists(resolvedFolderPath))
            return Task.FromResult(new List<FileStorageItem>());

        var enumeration = new EnumerationOptions
        {
            RecurseSubdirectories = isRecursive,
            AttributesToSkip = FileAttributes.ReparsePoint,
            IgnoreInaccessible = false
        };
        var files = new DirectoryInfo(resolvedFolderPath)
            .EnumerateFiles("*", enumeration)
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

    private static string ResolveFilePath(string fullPath)
    {
        var path = FileStoragePath.ResolveFullPath(fullPath);
        RejectReparsePoint(Path.GetDirectoryName(path));
        RejectReparsePoint(path);
        return path;
    }

    private static void RejectReparsePoint(string path)
    {
        try
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("File storage does not follow symbolic links or reparse points at the target.");
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
    }
}
