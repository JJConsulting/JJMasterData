using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.IO.Storage;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileService(FormFileManagerFactory formFileManagerFactory, IFileStorage fileStorage)
{
    public async Task SaveFormTemporaryFilesAsync(FormElement formElement, Dictionary<string, object> values)
    {
        var uploadFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        foreach (var field in uploadFields)
        {
            var folderKey = fileStorage.GetFolderKey(formElement, field, values);
            var manager = formFileManagerFactory.Create($"{field.Name}-upload-view-files");
            
            await manager.PromoteTemporaryFilesAsync(folderKey, deleteExistingFiles: !field.DataFile.MultipleFile);
        }
    }

    public async Task DeleteFilesAsync(FormElement formElement, Dictionary<string, object> primaryKeys)
    {
        var fileFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (fileFields.Count == 0)
            return;
        
        foreach (var field in fileFields)
        {
            var manager = formFileManagerFactory.Create($"{field.Name}-upload-view-files");
            manager.FolderKey = fileStorage.GetFolderKey(formElement, field, primaryKeys);
            await manager.DeleteAllAsync();
        }
    }
}
