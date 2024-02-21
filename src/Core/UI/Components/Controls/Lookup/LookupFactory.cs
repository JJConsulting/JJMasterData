using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class LookupFactory(
        IHttpRequest httpRequest,
        FormValuesService formValuesService,
        LookupService lookupService,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        RouteContextFactory routeContextFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJLookup>
{
    private IHttpRequest HttpRequest { get; } = httpRequest;
    private FormValuesService FormValuesService { get; } = formValuesService;
    private LookupService LookupService { get; } = lookupService;
    private IComponentFactory ComponentFactory { get; } = componentFactory;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private RouteContextFactory RouteContextFactory { get; } = routeContextFactory;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;


    public JJLookup Create()
    {
        return new JJLookup(
            null,
            null,
            HttpRequest,
            RouteContextFactory,
            FormValuesService,
            EncryptionService,
            LookupService,
            StringLocalizer,
            ComponentFactory);
    }

    public JJLookup Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        var lookup = new JJLookup(
            field,
            controlContext,
            HttpRequest,
            RouteContextFactory,
            FormValuesService,
            EncryptionService,
            LookupService,
            StringLocalizer,
            ComponentFactory);

        lookup.ElementName = formElement.Name;
        lookup.ParentElementName = formElement.ParentName;
        
        return lookup;
    }

    
}