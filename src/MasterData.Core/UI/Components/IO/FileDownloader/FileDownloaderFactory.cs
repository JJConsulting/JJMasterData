using System.Collections.Generic;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Storage;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public sealed class FileDownloaderFactory(IHttpContextAccessor httpContext,
        IFileStorage fileStorage,
        ITemporaryFileStore temporaryFileStore,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public JJFileDownloader Create()
    {
        return new JJFileDownloader(httpContext, fileStorage, temporaryFileStore, encryptionService, stringLocalizer);
    }

    public JJFileDownloader Create(FileStorageItemKey file)
    {
        var downloader = Create();
        downloader.File = file;
        return downloader;
    }

    public JJFileDownloader Create(FormElement formElement, FormElementField field, Dictionary<string, object> values, string fileName, bool isTemporary = false)
    {
        var storage = isTemporary ? temporaryFileStore : fileStorage;
        var folderPath = storage.GetFolderPath(formElement, field, values);
        return Create(new FileStorageItemKey(folderPath, fileName, isTemporary));
    }
}
