using JJMasterData.Commons.Security.Cryptography.Abstractions;

namespace JJMasterData.Core.UI.Routing;

public class RouteContextFactory(IHttpContextAccessor httpContextAccessor, IEncryptionService encryptionService)
{
    public RouteContext Create()
    {
        var queryString = httpContextAccessor.HttpContext?.Request.Query;
        if (queryString?.TryGetValue("routeContext", out var encryptedQueryString) == true)
        {
            return encryptionService.DecryptRouteContext(encryptedQueryString);
        }

        return new RouteContext();
    }
}
