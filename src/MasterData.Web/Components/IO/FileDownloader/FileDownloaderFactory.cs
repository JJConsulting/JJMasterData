using System.Collections.Generic;
using JJConsulting.MasterData.Storage.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Commons.Storage;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Web.Components;

public sealed class FileDownloaderFactory(IHttpContextAccessor httpContext,
        IFileStorage fileStorage,
        IEncryptionService encryptionService)
{
    public JJFileDownloader Create()
    {
        return new JJFileDownloader(httpContext, fileStorage, encryptionService);
    }

    public JJFileDownloader Create(string fullPath)
    {
        var downloader = Create();
        downloader.FullPath = fullPath;
        return downloader;
    }
    
    public JJFileDownloader Create(FormElement formElement, FormElementField field, Dictionary<string, object?> values, string fileName, bool isTemporary = false)
    {
        var folderPath = FileStoragePath.GetFolderPath(formElement, field, values);
        var fullPath = FileStoragePath.Combine(folderPath, fileName);
        
        return Create(fullPath);
    }
}
