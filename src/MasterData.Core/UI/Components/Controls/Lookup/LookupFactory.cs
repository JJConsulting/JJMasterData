using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class LookupFactory(
        IHttpContextAccessor httpRequest,
        FormValuesService formValuesService,
        LookupService lookupService,
        IComponentFactory componentFactory,
        DataProtectionService encryptionService,
        RouteContextFactory routeContextFactory,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJLookup>
{
    public JJLookup Create()
    {
        return new JJLookup(
            null!,
            null!,
            null!,
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
        return new JJLookup(
            formElement,
            field,
            controlContext,
            httpRequest,
            routeContextFactory,
            formValuesService,
            encryptionService,
            lookupService,
            stringLocalizer,
            componentFactory);
    }
}
