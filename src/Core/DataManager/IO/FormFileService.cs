using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileService(FormFileManagerFactory formFileManagerFactory)
{
    public void SaveFormMemoryFiles(FormElement formElement, Dictionary<string, object> primaryKeys)
    {
        var uploadFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        var pathBuilder = new FormFilePathBuilder(formElement);
        foreach (var field in uploadFields)
        {
            var folderPath = pathBuilder.GetFolderPath(field, primaryKeys);
            var manager = formFileManagerFactory.Create($"{field.Name}-upload-view-files");
            manager.SaveMemoryFiles(folderPath);
        }
    }

    public void DeleteFiles(FormElement formElement, Dictionary<string, object> primaryKeys)
    {
        var fileFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (fileFields.Count == 0)
            return;
        
        foreach (var field in fileFields)
        {
            var manager = formFileManagerFactory.Create($"{field.Name}-upload-view-files");
            manager.FolderPath = new FormFilePathBuilder(formElement).GetFolderPath(field, primaryKeys);
            manager.DeleteAll();
        }
    }
}