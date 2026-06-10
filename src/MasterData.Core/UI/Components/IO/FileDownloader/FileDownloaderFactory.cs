using System.Collections.Generic;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public sealed class FileDownloaderFactory(IHttpContextAccessor httpContext,
        IFileStorage fileStorage,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public JJFileDownloader Create()
    {
        return new JJFileDownloader(httpContext, fileStorage, encryptionService, stringLocalizer);
    }

    public JJFileDownloader Create(string fullPath)
    {
        var downloader = Create();
        downloader.FullPath = fullPath;
        return downloader;
    }
    
    public JJFileDownloader Create(FormElement formElement, FormElementField field, Dictionary<string, object> values, string fileName, bool isTemporary = false)
    {
        var folderPath = FileStoragePath.GetFolderPath(formElement, field, values);
        var fullPath = FileStoragePath.Combine(folderPath, fileName);
        
        return Create(fullPath);
    }
}
