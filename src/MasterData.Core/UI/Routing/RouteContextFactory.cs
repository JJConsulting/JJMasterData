#nullable disable warnings
using JJMasterData.Commons.Security;

namespace JJMasterData.Core.UI.Routing;

public class RouteContextFactory(IHttpContextAccessor httpContextAccessor, DataProtectionService dataProtectionService)
{
    public RouteContext Create()
    {
        var queryString = httpContextAccessor.HttpContext?.Request.Query;
        if (queryString?.TryGetValue("routeContext", out var encryptedQueryString) == true)
        {
            return dataProtectionService.UnprotectRouteContext(encryptedQueryString);
        }

        return new RouteContext();
    }
}
