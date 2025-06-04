#if NETFRAMEWORK
using System.Security.Claims;
using System.Web;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http.SystemWeb;

public class SystemWebClaimsPrincipalWrapper : IClaimsPrincipalAccessor
{
    public ClaimsPrincipal User { get; }
    
    public SystemWebClaimsPrincipalWrapper()
    {
        if (HttpContext.Current == null)
            return;
        if (HttpContext.Current.User == null)
            return;
        
        User = HttpContext.Current.User as ClaimsPrincipal;
    }
}
#endif