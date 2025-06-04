using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Routing;

public class RouteContextFactory(IQueryString queryString, IEncryptionService encryptionService)
{
    public RouteContext Create()
    {
        if (queryString.TryGetValue("routeContext", out var encryptedQueryString))
        {
            return encryptionService.DecryptRouteContext(encryptedQueryString);
        }

        return new RouteContext();
    }
}