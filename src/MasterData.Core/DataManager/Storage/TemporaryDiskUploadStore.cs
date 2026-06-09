using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Storage;

public sealed class TemporaryDiskUploadStore : DiskFileStorage, ITemporaryUploadStore
{
    private readonly string _rootPath = Path.Combine(Path.GetTempPath(), "jjmasterdata", "uploads");

    public string CreateDraftId()
    {
        CleanupExpiredCore(TimeSpan.FromHours(12));
        return Guid.NewGuid().ToString("N");
    }

    public string GetDraftFolderKey(string draftId)
    {
        draftId = NormalizeDraftId(draftId);
        if (string.IsNullOrWhiteSpace(draftId))
            throw new ArgumentNullException(nameof(draftId));

        return draftId;
    }

    public Task SaveUploadAsync(string draftId, string fileName, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default)
    {
        return SaveAsync(GetDraftFolderKey(draftId), fileName, content, replaceIfExists, cancellationToken);
    }

    public async Task PromoteAsync(string draftId, IFileStorage destinationStorage, string destinationFolderKey, bool deleteExistingFiles = false, CancellationToken cancellationToken = default)
    {
        var draftFolderKey = GetDraftFolderKey(draftId);
        var files = await ListAsync(draftFolderKey, cancellationToken);
        if (files.Count == 0)
            return;

        if (deleteExistingFiles)
            await destinationStorage.DeleteFolderAsync(destinationFolderKey, cancellationToken);

        foreach (var file in files)
        {
            await using var stream = await OpenReadAsync(draftFolderKey, file.FileName, cancellationToken);
            await destinationStorage.SaveAsync(destinationFolderKey, file.FileName, stream, true, cancellationToken);
        }

        await DeleteFolderAsync(draftFolderKey, cancellationToken);
    }

    public Task CleanupExpiredAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        CleanupExpiredCore(maxAge);
        return Task.CompletedTask;
    }

    private void CleanupExpiredCore(TimeSpan maxAge)
    {
        if (!Directory.Exists(_rootPath))
            return;

        var threshold = DateTime.Now.Subtract(maxAge);
        foreach (var directory in new DirectoryInfo(_rootPath).EnumerateDirectories())
        {
            if (directory.LastWriteTime < threshold)
                directory.Delete(true);
        }
    }

    protected override string ResolveFolderPath(string folderKey)
    {
        var draftId = GetDraftFolderKey(folderKey);
        return Path.Combine(_rootPath, draftId);
    }

    private static string NormalizeDraftId(string draftId)
    {
        if (string.IsNullOrWhiteSpace(draftId))
            return draftId;

        draftId = draftId.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
        draftId = Path.GetFileName(draftId);

        if (!Guid.TryParseExact(draftId, "N", out _))
            throw new ArgumentException("Invalid temporary upload draft id.", nameof(draftId));

        return draftId;
    }
}
