using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal class UploadViewFactory(IHttpContext currentContext,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    : IComponentFactory<JJUploadView>
{
    public ILoggerFactory LoggerFactory { get; } = loggerFactory;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IHttpContext CurrentContext { get; } = currentContext;
    private IComponentFactory ComponentFactory { get; } = componentFactory;

    public JJUploadView Create()
    {
        return new JJUploadView(
            CurrentContext, 
            ComponentFactory,
            EncryptionService, 
            StringLocalizer,
            LoggerFactory);
    }
}