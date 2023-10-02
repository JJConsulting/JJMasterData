using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Routing;

public class RouteContextFactory
{
    private IQueryString QueryString { get; }
    private IEncryptionService EncryptionService { get; }

    public RouteContextFactory(IQueryString queryString, IEncryptionService encryptionService)
    {
        QueryString = queryString;
        EncryptionService = encryptionService;
    }
    
    public RouteContext Create()
    {
        if (QueryString.TryGetValue("routeContext", out var encryptedQueryString))
        {
            return EncryptionService.DecryptRouteContext(encryptedQueryString);
        }

        return new RouteContext();
    }
}