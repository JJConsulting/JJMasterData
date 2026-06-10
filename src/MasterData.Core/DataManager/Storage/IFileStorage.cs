using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Storage;

public interface IFileStorage
{
    string GetFolderPath(FormElement formElement, FormElementField field, Dictionary<string, object> values);
    Task SaveAsync(string fullPath, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string fullPath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fullPath, CancellationToken cancellationToken = default);
    Task DeleteFolderAsync(string folderPath, CancellationToken cancellationToken = default);
    Task MoveAsync(string currentFullPath, string newFullPath, CancellationToken cancellationToken = default);
    Task<List<FileStorageItem>> ListAsync(string folderPath, CancellationToken cancellationToken = default);
}
