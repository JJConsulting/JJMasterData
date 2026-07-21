using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.DataManager.Services;
using JJMasterData.Web.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Components;

internal sealed class LookupFactory(
        IHttpContextAccessor httpRequest,
        FormValuesService formValuesService,
        LookupService lookupService,
        LookupRequestService lookupRequestService,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
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
            lookupRequestService,
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
            lookupRequestService,
            stringLocalizer,
            componentFactory);
    }
}
