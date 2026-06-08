using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.IO.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public sealed class FileDownloaderFactory(IHttpContextAccessor httpContext,
        IFileStorage fileStorage,
        ITemporaryUploadStore temporaryUploadStore,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
{
    public JJFileDownloader Create()
    {
        return new JJFileDownloader(httpContext, fileStorage, temporaryUploadStore, encryptionService, stringLocalizer,
            loggerFactory.CreateLogger<JJFileDownloader>());
    }
   
}
