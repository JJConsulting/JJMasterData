using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal sealed class UploadViewFactory(IHttpContext currentContext,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    : IComponentFactory<JJUploadView>
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