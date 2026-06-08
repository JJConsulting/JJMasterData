using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataManager.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

public sealed class UploadViewFactory(IHttpContextAccessor currentContext,
        IComponentFactory componentFactory,
        FormFileManagerFactory formFileManagerFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
{
    public JJUploadView Create()
    {
        return new JJUploadView(
            currentContext, 
            componentFactory,
            formFileManagerFactory,
            encryptionService, 
            stringLocalizer,
            loggerFactory);
    }
}
