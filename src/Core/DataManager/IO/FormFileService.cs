using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.IO;

public class FormFileService(FormFileManagerFactory formFileManagerFactory)
{
    private FormFileManagerFactory FormFileManagerFactory { get; } = formFileManagerFactory;

    public void SaveFormMemoryFiles(FormElement formElement, Dictionary<string, object> primaryKeys)
    {
        var uploadFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        var pathBuilder = new FormFilePathBuilder(formElement);
        foreach (var field in uploadFields)
        {
            string folderPath = pathBuilder.GetFolderPath(field, primaryKeys);
            var manager = FormFileManagerFactory.Create($"{field.Name}-upload-view-files");
            manager.SaveMemoryFiles(folderPath);
        }
    }

    public void DeleteFiles(FormElement formElement, Dictionary<string, object> primaryKeys)
    {
        var uploadFields = formElement.Fields.FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;
        
        foreach (var field in uploadFields)
        {
            var manager = FormFileManagerFactory.Create($"{field.Name}-upload-view-files");
            manager.DeleteAll();
        }
    }
}