using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

internal class UploadViewFactory : IComponentFactory<JJUploadView>
{
    public ILoggerFactory LoggerFactory { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private IHttpContext CurrentContext { get; }
    private IComponentFactory ComponentFactory { get; }
    
    public UploadViewFactory(
        IHttpContext currentContext,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
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