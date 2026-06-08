using System.Threading.Tasks;

namespace JJMasterData.Core.DataManager.IO.Storage;

public interface IFileReferenceStore
{
    string Create(FileStorageReference reference);
    Task<FileStorageReference> ResolveAsync(string token);
}
