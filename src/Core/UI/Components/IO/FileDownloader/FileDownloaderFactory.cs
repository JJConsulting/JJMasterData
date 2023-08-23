using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Cryptography.Abstractions;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

internal class FileDownloaderFactory : IComponentFactory<JJFileDownloader>
{
    private IHttpContext HttpContext { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private ILoggerFactory LoggerFactory { get; }

    public FileDownloaderFactory(
        IHttpContext httpContext, 
        JJMasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
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
    
    public JJFileDownloader CreateFileDownloader(string filePath)
    {
        return new JJFileDownloader(HttpContext,UrlHelper,EncryptionService,StringLocalizer,LoggerFactory.CreateLogger<JJFileDownloader>())
        {
            FilePath = filePath
        };
    }


}