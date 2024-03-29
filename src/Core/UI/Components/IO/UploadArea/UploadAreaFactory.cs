using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class UploadAreaFactory(IHttpContext httpContext,
        UploadAreaService uploadAreaService,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IComponentFactory<JJUploadArea>
{
    private IHttpContext HttpContext { get; } = httpContext;
    private UploadAreaService UploadAreaService { get; } = uploadAreaService;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    public JJUploadArea Create()
    {
        return new JJUploadArea(HttpContext,UploadAreaService,EncryptionService, StringLocalizer);
    }
}   