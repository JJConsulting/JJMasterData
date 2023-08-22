using System.Linq;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Http;

/// <summary>
/// Wrapper to HttpContext that works on both .NET 4.8 and .NET 6+
/// </summary>
public class JJHttpContext : IHttpContext
{
    public bool IsPost => Request.HttpMethod.Equals("POST");

    public IHttpSession Session { get; }

    public IHttpRequest Request { get; }
    

#if NET || NETSTANDARD
    private Microsoft.AspNetCore.Http.HttpContext HttpContext { get; }
    
    public JJHttpContext(
        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor,
        IHttpSession session,
        IHttpRequest request)
    {
        HttpContext = httpContextAccessor.HttpContext;
        Session = session;
        Request = request;
    }
#endif
    
#if NETFRAMEWORK
    public JJHttpContext(IHttpRequest request,IHttpSession session)
    {
        Request = request;
        Session = session;
    }
#endif


    /// <summary>
    /// Verify if the current User has a valid Claims property.
    /// </summary>
    /// <returns></returns>
    public bool HasClaimsIdentity()
    {
#if NETFRAMEWORK
        return System.Web.HttpContext.Current.User.Identity is System.Security.Claims.ClaimsIdentity;
#else
        var claims = HttpContext.User.Claims;
        return claims.Any();
#endif
    }
    /// <summary>
    /// Returns a User claim.
    /// </summary>
    /// <param name="key">The claim type.</param>
    /// <returns></returns>
    public string GetClaim(string key)
    {
#if NETFRAMEWORK
        if (System.Web.HttpContext.Current.User.Identity is not System.Security.Claims.ClaimsIdentity identity)
            return null;

        var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
        return claim?.Value;
#else
        return HttpContext.User.Claims.FirstOrDefault(claim => claim.Type == key)?.Value!;
#endif
    }

    public bool HasContext()
    {
        return true;
    }
}