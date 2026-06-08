using JJMasterData.Commons.Security.Cryptography.Abstractions;
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
   
}
