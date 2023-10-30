using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class UploadAreaFactory : IComponentFactory<JJUploadArea>
{
    private IHttpContext HttpContext { get; }
    private UploadAreaService UploadAreaService { get; }
    private MasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }

    public UploadAreaFactory(
        IHttpContext httpContext,
        UploadAreaService uploadAreaService, 
        MasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        UploadAreaService = uploadAreaService;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }

    public JJUploadArea Create()
    {
        return new JJUploadArea(HttpContext,UploadAreaService,UrlHelper,EncryptionService, StringLocalizer);
    }
}   