using System;
using System.Collections.Generic;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.IO.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public sealed class FileDownloaderFactory(IHttpContextAccessor httpContext,
        IFileStorage fileStorage,
        ITemporaryUploadStore temporaryUploadStore,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
{
    public JJFileDownloader Create()
    {
        return new JJFileDownloader(httpContext, fileStorage, temporaryUploadStore, encryptionService, stringLocalizer);
    }

    public JJFileDownloader Create(FileStorageReference fileReference)
    {
        var downloader = Create();
        downloader.FileReference = fileReference;
        return downloader;
    }

    public JJFileDownloader Create(FormElement formElement, FormElementField field, Dictionary<string, object> values, string fileName, bool isTemporary = false)
    {
        var storage = isTemporary ? temporaryUploadStore : fileStorage;
        var folderKey = storage.GetFolderKey(formElement, field, values);
        return Create(FileStorageReference.Create(folderKey, fileName, isTemporary));
    }
}
