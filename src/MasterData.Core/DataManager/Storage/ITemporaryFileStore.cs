using System.Threading;
using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.Storage;

public interface ITemporaryFileStore : IFileStorage
{
    string CreateDraftId();
    string GetDraftFolderPath(string draftId);
    Task PromoteAsync(string draftId, IFileStorage destinationStorage, string destinationFolderPath, bool deleteExistingFiles = false, CancellationToken cancellationToken = default);
}
