using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Commons.Storage;

public static class FileStorageExtensions
{
    private static readonly string RootPath = Path.Combine(Path.GetTempPath(), "jjmasterdata", "uploads");

    extension(IFileStorage fileStorage)
    {
        public string CreateDraftId()
        {
            ArgumentNullException.ThrowIfNull(fileStorage);

            CleanupExpired(TimeSpan.FromHours(12));
            return Guid.NewGuid().ToString("N");
        }

        public string GetDraftFolderPath(string draftId)
        {
            ArgumentNullException.ThrowIfNull(fileStorage);

            draftId = NormalizeDraftId(draftId);
            if (string.IsNullOrWhiteSpace(draftId))
                throw new ArgumentNullException(nameof(draftId));

            return ResolveDraftFolderPath(draftId);
        }

        public async Task PromoteAsync(string draftId,
            IFileStorage destinationStorage,
            string destinationFolderPath,
            bool deleteExistingFiles = false,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(fileStorage);
            ArgumentNullException.ThrowIfNull(destinationStorage);

            var draftFolderPath = fileStorage.GetDraftFolderPath(draftId);
            var files = await fileStorage.ListAsync(draftFolderPath, cancellationToken);
            if (files.Count == 0)
                return;

            if (deleteExistingFiles)
                await destinationStorage.DeleteFolderAsync(destinationFolderPath, cancellationToken);

            foreach (var file in files)
            {
                await using var stream = await fileStorage.OpenReadAsync(file.FullPath, cancellationToken);
                var fullPath = FileStoragePath.Combine(destinationFolderPath, file.FileName);
                await destinationStorage.SaveAsync(fullPath, stream, true, cancellationToken);
            }

            await fileStorage.DeleteFolderAsync(draftFolderPath, cancellationToken);
        }
    }

    private static void CleanupExpired(TimeSpan maxAge)
    {
        if (!Directory.Exists(RootPath))
            return;

        var threshold = DateTime.Now.Subtract(maxAge);
        foreach (var directory in new DirectoryInfo(RootPath).EnumerateDirectories())
        {
            if (directory.LastWriteTime < threshold)
                directory.Delete(true);
        }
    }

    private static string ResolveDraftFolderPath(string folderPath)
    {
        var draftId = NormalizeDraftId(folderPath);
        return Path.Combine(RootPath, draftId);
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
