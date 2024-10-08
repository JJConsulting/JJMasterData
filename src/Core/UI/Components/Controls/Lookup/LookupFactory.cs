using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class LookupFactory(
        IHttpRequest httpRequest,
        FormValuesService formValuesService,
        LookupService lookupService,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        RouteContextFactory routeContextFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJLookup>
{
    
    public JJLookup Create()
    {
        return new JJLookup(
            null,
            null,
            httpRequest,
            routeContextFactory,
            formValuesService,
            encryptionService,
            lookupService,
            stringLocalizer,
            componentFactory);
    }

    public JJLookup Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        var lookup = new JJLookup(
            field,
            controlContext,
            httpRequest,
            routeContextFactory,
            formValuesService,
            encryptionService,
            lookupService,
            stringLocalizer,
            componentFactory);

        lookup.ElementName = formElement.Name;
        lookup.ParentElementName = formElement.ParentName;
        
        return lookup;
    }

    
}