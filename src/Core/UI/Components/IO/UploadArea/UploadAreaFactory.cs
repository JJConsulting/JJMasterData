using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class UploadAreaFactory(IHttpContext httpContext,
        UploadAreaService uploadAreaService,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IComponentFactory<JJUploadArea>
{
    public JJUploadArea Create()
    {
        return new JJUploadArea(httpContext,uploadAreaService,encryptionService, stringLocalizer);
    }
}   