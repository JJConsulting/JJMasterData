using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Routing;

public class RouteContextFactory(IQueryString queryString, IEncryptionService encryptionService)
{
    private IQueryString QueryString { get; } = queryString;
    private IEncryptionService EncryptionService { get; } = encryptionService;

    public RouteContext Create()
    {
        if (QueryString.TryGetValue("routeContext", out var encryptedQueryString))
        {
            return EncryptionService.DecryptRouteContext(encryptedQueryString);
        }

        return new RouteContext();
    }
}