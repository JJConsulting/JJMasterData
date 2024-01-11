using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal class FileDownloaderFactory(IHttpContext httpContext,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    : IComponentFactory<JJFileDownloader>
{
    private IHttpContext HttpContext { get; } = httpContext;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private ILoggerFactory LoggerFactory { get; } = loggerFactory;

    public JJFileDownloader Create()
    {
        return new JJFileDownloader(HttpContext, EncryptionService, StringLocalizer,
            LoggerFactory.CreateLogger<JJFileDownloader>());
    }
   
}