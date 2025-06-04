using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal sealed class FileDownloaderFactory(IHttpContext httpContext,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    : IComponentFactory<JJFileDownloader>
{
    public JJFileDownloader Create()
    {
        return new JJFileDownloader(httpContext, encryptionService, stringLocalizer,
            loggerFactory.CreateLogger<JJFileDownloader>());
    }
   
}