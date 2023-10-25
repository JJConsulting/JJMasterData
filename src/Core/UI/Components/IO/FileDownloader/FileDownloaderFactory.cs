using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal class FileDownloaderFactory : IComponentFactory<JJFileDownloader>
{
    private IHttpContext HttpContext { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }

    public FileDownloaderFactory(
        IHttpContext httpContext, 
        JJMasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory
        )
    {
        HttpContext = httpContext;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

    public JJFileDownloader Create()
    {
        return new JJFileDownloader(HttpContext, UrlHelper, EncryptionService, StringLocalizer,
            LoggerFactory.CreateLogger<JJFileDownloader>());
    }
   
}