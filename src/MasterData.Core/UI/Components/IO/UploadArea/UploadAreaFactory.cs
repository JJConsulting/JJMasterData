using JJMasterData.Commons.Security;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.UI.Components;

public sealed class UploadAreaFactory(IHttpContextAccessor httpContext,
        UploadAreaManager uploadAreaManager,
        DataProtectionService encryptionService,
        IOptions<FormOptions> requestLengthService,
        IStringLocalizer<MasterDataResources> stringLocalizer)

{
    public JJUploadArea Create()
    {
        return new JJUploadArea(httpContext,uploadAreaManager,encryptionService,requestLengthService, stringLocalizer);
    }
}   
