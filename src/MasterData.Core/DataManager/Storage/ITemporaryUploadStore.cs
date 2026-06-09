using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Storage;

public interface ITemporaryUploadStore : IFileStorage
{
    string CreateDraftId();
    string GetDraftFolderKey(string draftId);
    Task SaveUploadAsync(string draftId, string fileName, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default);
    Task PromoteAsync(string draftId, IFileStorage destinationStorage, string destinationFolderKey, bool deleteExistingFiles = false, CancellationToken cancellationToken = default);
    Task CleanupExpiredAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
}
