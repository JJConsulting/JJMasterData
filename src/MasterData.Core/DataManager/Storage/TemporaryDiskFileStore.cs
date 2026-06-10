using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Storage;

public sealed class TemporaryDiskFileStore : DiskFileStorage, ITemporaryFileStore
{
    private readonly string _rootPath = Path.Combine(Path.GetTempPath(), "jjmasterdata", "uploads");

    public string CreateDraftId()
    {
        CleanupExpired(TimeSpan.FromHours(12));
        return Guid.NewGuid().ToString("N");
    }

    public string GetDraftFolderPath(string draftId)
    {
        draftId = NormalizeDraftId(draftId);
        if (string.IsNullOrWhiteSpace(draftId))
            throw new ArgumentNullException(nameof(draftId));

        return draftId;
    }

    public async Task PromoteAsync(string draftId, IFileStorage destinationStorage, string destinationFolderPath, bool deleteExistingFiles = false, CancellationToken cancellationToken = default)
    {
        var draftFolderPath = GetDraftFolderPath(draftId);
        var files = await ListAsync(draftFolderPath, cancellationToken);
        if (files.Count == 0)
            return;

        if (deleteExistingFiles)
            await destinationStorage.DeleteFolderAsync(destinationFolderPath, cancellationToken);

        foreach (var file in files)
        {
            await using var stream = await OpenReadAsync(file.FullPath, cancellationToken);
            var fullPath = FileStoragePath.Combine(destinationFolderPath, file.FileName);
            await destinationStorage.SaveAsync(fullPath, stream, true, cancellationToken);
        }

        await DeleteFolderAsync(draftFolderPath, cancellationToken);
    }

    private void CleanupExpired(TimeSpan maxAge)
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

    protected override string ResolveFolderPath(string folderPath)
    {
        var draftId = GetDraftFolderPath(folderPath);
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
