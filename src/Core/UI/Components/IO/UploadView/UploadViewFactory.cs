using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.UI.Components;

internal class UploadViewFactory : IComponentFactory<JJUploadView>
{
    public ILoggerFactory LoggerFactory { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; }
    private IHttpContext CurrentContext { get; }
    private IComponentFactory ComponentFactory { get; }
    
    public UploadViewFactory(
        IHttpContext currentContext,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        ComponentFactory = componentFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

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