using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

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