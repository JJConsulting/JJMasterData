using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public sealed class UploadViewFactory(IHttpContextAccessor currentContext,
        IComponentFactory componentFactory,
        FormFileService formFileService,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
{
    public JJUploadView Create()
    {
        return new JJUploadView(
            currentContext, 
            componentFactory,
            formFileService,
            encryptionService, 
            stringLocalizer,
            loggerFactory);
    }
}
