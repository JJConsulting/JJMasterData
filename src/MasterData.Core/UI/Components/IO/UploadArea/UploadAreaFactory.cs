using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

public sealed class UploadAreaFactory(IHttpContextAccessor httpContext,
        UploadAreaManager uploadAreaManager,
        IEncryptionService encryptionService,
        IOptions<FormOptions> requestLengthService,
        IStringLocalizer<MasterDataResources> stringLocalizer)

{
    public JJUploadArea Create()
    {
        return new JJUploadArea(httpContext,uploadAreaManager,encryptionService,requestLengthService, stringLocalizer);
    }
}   
