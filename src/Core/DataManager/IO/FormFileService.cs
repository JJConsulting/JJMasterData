using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Core.DataManager;

public class FormFileService : IFormFileService
{
    private FormFileManagerFactory FormFileManagerFactory { get; }

    public FormFileService(FormFileManagerFactory formFileManagerFactory)
    {
        FormFileManagerFactory = formFileManagerFactory;
    }

    public void SaveFormMemoryFiles(FormElement formElement, IDictionary<string, object> primaryKeys)
    {
        var uploadFields = formElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
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

    public void DeleteFiles(FormElement formElement, IDictionary<string, object> primaryKeys)
    {
        var uploadFields = formElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;
        
        foreach (var field in uploadFields)
        {
            var manager = FormFileManagerFactory.Create($"{field.Name}-upload-view-files");
            manager.DeleteAll();
        }
    }
}