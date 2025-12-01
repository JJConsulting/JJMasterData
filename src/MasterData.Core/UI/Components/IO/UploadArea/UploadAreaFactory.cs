using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public sealed class UploadAreaFactory(IHttpContext httpContext,
        UploadAreaService uploadAreaService,
        IEncryptionService encryptionService,
        IRequestLengthService requestLengthService,
        IStringLocalizer<MasterDataResources> stringLocalizer)

{
    public JJUploadArea Create()
    {
        return new JJUploadArea(httpContext,uploadAreaService,encryptionService,requestLengthService, stringLocalizer);
    }
}   