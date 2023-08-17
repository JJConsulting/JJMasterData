// ReSharper disable RedundantUsingDirective

using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Web.Http;

/// <summary>
/// Wrapper to HttpContext that works on both .NET Framework and .NET Core
/// </summary>
public class JJHttpContext
{
    private static JJHttpContext _instance;


    private JJHttpContext()
    {
    }

    public static JJHttpContext GetInstance()
    {
        return _instance ??= new JJHttpContext();
    }
#if NETFRAMEWORK || NETSTANDARD
    internal static System.Web.HttpContext SystemWebCurrent => System.Web.HttpContext.Current;
#endif
#if NETCOREAPP 
    internal static System.Web.HttpContext SystemWebCurrent => Commons.DI.JJService.Provider?.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>()?.HttpContext;
#endif
#if NETCOREAPP || NETSTANDARD
    internal static Microsoft.AspNetCore.Http.HttpContext AspNetCoreCurrent
    {
        get
        {
            var accessor = Commons.DI.JJService.Provider?.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>();

            if (accessor?.HttpContext != null) return accessor.HttpContext;

            var context = new Microsoft.AspNetCore.Http.HttpContextAccessor().HttpContext;

            return context;

        }
    }

#endif
    private JJResponse _response;
    private JJRequest _request;
    private JJSession _session;

    public bool IsPostBack => HasContext() && Request.HttpMethod.Equals("POST");

    public JJSession Session => _session ??=new();

    public JJRequest Request => _request ??= new();

    public JJResponse Response => _response ??= new();

    
    /// <summary>
    /// Verify if context is valid.
    /// </summary>
    /// <returns></returns>
    public bool HasContext()
    {
#if NETCOREAPP
        return AspNetCoreCurrent != null;
#else
        return SystemWebCurrent != null;
#endif
    }

    /// <summary>
    /// Verify if the current User has a valid Claims property.
    /// </summary>
    /// <returns></returns>
    public bool HasClaimsIdentity()
    {
#if NETFRAMEWORK
        return SystemWebCurrent != null && SystemWebCurrent.User?.Identity is System.Security.Claims.ClaimsIdentity;
#else
        var claims = AspNetCoreCurrent?.User?.Claims;
        return claims != null && claims.Any();
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
        if (SystemWebCurrent.User.Identity is not System.Security.Claims.ClaimsIdentity identity)
            return null;

        var claim = identity.Claims.FirstOrDefault(c => c.Type == key);
        return claim?.Value;
#else
        return AspNetCoreCurrent.User.Claims.FirstOrDefault(claim => claim.Type == key)?.Value;
#endif
    }
}
