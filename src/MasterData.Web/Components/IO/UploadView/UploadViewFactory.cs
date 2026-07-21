#nullable disable warnings
using System.Collections.Generic;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Web.Components;

public sealed class UploadViewFactory(IHttpContextAccessor currentContext,
        IComponentFactory componentFactory,
        UploadViewManager manager,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
{
    public JJUploadView Create()
    {
        return new JJUploadView(
            currentContext, 
            componentFactory,
            manager,
            encryptionService, 
            stringLocalizer,
            loggerFactory);
    }


    public JJUploadView Create(
        FormElement formElement, 
        FormElementField field, 
        Dictionary<string, object> values)
    {
        var uploadView = Create();

        uploadView.Name = $"{field.Name}-upload-view";
        uploadView.ParentName = formElement.ParentName; //Atençao verificar se he nulo no JJTextFile
        uploadView.Title = string.Empty;
        uploadView.AutoSave = false;
        uploadView.DeletedFilesInputName = UploadViewManager.GetDeletedFilesInputName(field);
        
        uploadView.RenameAction.SetVisible(true);
            
        if (DataHelper.ContainsPkValues(formElement, values))
            uploadView.FolderPath = FileStoragePath.GetFolderPath(formElement, field, values);
        
        var dataFile = field.DataFile!;
        uploadView.UploadArea.Multiple = dataFile.MultipleFile;
        uploadView.UploadArea.MaxFileSize = dataFile.MaxFileSize;
        uploadView.UploadArea.ShowFileSize = dataFile.ExportAsLink;
        uploadView.UploadArea.AllowedTypes = dataFile.AllowedTypes;
        uploadView.UploadArea.EnableCopyPaste = dataFile.AllowPasting;
        uploadView.UploadArea.RouteContext.ElementName = formElement.Name;
        uploadView.UploadArea.RouteContext.ParentElementName = formElement.ParentName;
        uploadView.UploadArea.RouteContext.ComponentContext = ComponentContext.TextFileFileUpload;
        uploadView.UploadArea.QueryStringParams["fieldName"] = field.Name;
        uploadView.ViewGallery = dataFile.ViewGallery;
        
        return uploadView;
    }
}
