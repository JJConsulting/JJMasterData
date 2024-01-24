#if NET
#nullable enable
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace JJMasterData.Core.Http.AspNetCore;

public class UrlHelperWrapper(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
    : IMasterDataUrlHelper
{
    private IUrlHelper UrlHelper { get; } = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext!);

    public string Action(string? action = null,string? controller = null, object? values = null)
    {
        return UrlHelper.Action(action, controller, values)!;
    }
}
#endif  