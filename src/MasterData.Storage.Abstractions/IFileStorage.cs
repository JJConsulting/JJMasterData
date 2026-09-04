namespace JJConsulting.MasterData.Storage.Abstractions;

public interface IFileStorage
{
    Task<List<FileStorageItem>> ListAsync(string folderPath, bool isRecursive = false, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default);
    Task SaveAsync(string fullPath, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fullPath, CancellationToken cancellationToken = default);
    Task DeleteFolderAsync(string folderPath, CancellationToken cancellationToken = default);
    Task MoveAsync(string currentFullPath, string newFullPath, CancellationToken cancellationToken = default);
}
