using JJMasterData.Commons.Security.Cryptography.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public sealed class FileDownloaderFactory(IHttpContextAccessor httpContext,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
{
    public JJFileDownloader Create()
    {
        return new JJFileDownloader(httpContext, encryptionService, stringLocalizer,
            loggerFactory.CreateLogger<JJFileDownloader>());
    }
   
}