using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.IO.Storage;

public interface IFileStorage
{
    string GetFolderKey(FormElement formElement, FormElementField field, Dictionary<string, object> values);
    Task SaveAsync(string folderKey, string fileName, Stream content, bool replaceIfExists = true, CancellationToken cancellationToken = default);
    Task<Stream> OpenReadAsync(string folderKey, string fileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string folderKey, string fileName, CancellationToken cancellationToken = default);
    Task DeleteFolderAsync(string folderKey, CancellationToken cancellationToken = default);
    Task RenameAsync(string folderKey, string currentName, string newName, CancellationToken cancellationToken = default);
    Task<List<FileStorageItem>> ListAsync(string folderKey, CancellationToken cancellationToken = default);
}
