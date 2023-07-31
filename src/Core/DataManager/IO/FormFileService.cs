using System.Collections.Generic;
using System.Linq;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.DataManager;

public class FormFileService
{
    private FormFileManagerFactory FormFileManagerFactory { get; }

    public FormFileService(FormFileManagerFactory formFileManagerFactory)
    {
        FormFileManagerFactory = formFileManagerFactory;
    }
    internal void SaveFormMemoryFiles(FormElement formElement, IDictionary<string,dynamic> primaryKeys)
    {
        var uploadFields = formElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;

        var pathBuilder = new FormFilePathBuilder(formElement);
        foreach (var field in uploadFields)
        {
            string folderPath = pathBuilder.GetFolderPath(field, primaryKeys);
            var manager = FormFileManagerFactory.Create(field.Name + "_uploadview");
            manager.SaveMemoryFiles(folderPath);
        }
    }

    internal void DeleteFiles(FormElement formElement, IDictionary<string,dynamic> primaryKeys)
    {
        var uploadFields = formElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;
        
        foreach (var field in uploadFields)
        {
            var manager = FormFileManagerFactory.Create(field.Name + "_uploadview");
            manager.DeleteAll();
        }
    }
}