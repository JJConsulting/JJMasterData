using JJMasterData.Commons.Security.Cryptography.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public sealed class UploadViewFactory(IHttpContextAccessor currentContext,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
{
    public JJUploadView Create()
    {
        return new JJUploadView(
            currentContext, 
            componentFactory,
            encryptionService, 
            stringLocalizer,
            loggerFactory);
    }
}