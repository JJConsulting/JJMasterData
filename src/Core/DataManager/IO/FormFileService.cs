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
    private IHttpContext HttpContext { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }

    public FormFileService(IHttpContext httpContext, IStringLocalizer<JJMasterDataResources> stringLocalizer, ILoggerFactory loggerFactory)
    {
        HttpContext = httpContext;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
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
            var fileService = new FormFileManager(field.Name + "_uploadview", HttpContext, StringLocalizer, LoggerFactory.CreateLogger<FormFileManager>());
            fileService.SaveMemoryFiles(folderPath);
        }
    }

    internal void DeleteFiles(FormElement formElement, IDictionary<string,dynamic> primaryKeys)
    {
        var uploadFields = formElement.Fields.ToList().FindAll(x => x.Component == FormComponent.File);
        if (uploadFields.Count == 0)
            return;
        
        foreach (var field in uploadFields)
        {
            var fileService = new FormFileManager(field.Name + "_uploadview", HttpContext, StringLocalizer, LoggerFactory.CreateLogger<FormFileManager>());
            fileService.DeleteAll();
        }
    }
}