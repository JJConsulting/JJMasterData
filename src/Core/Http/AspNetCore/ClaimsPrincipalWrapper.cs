#if NET
using System.Security.Claims;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

public class ClaimsPrincipalWrapper : IClaimsPrincipalAccessor
{
    public ClaimsPrincipal User { get; }

    public ClaimsPrincipalWrapper(IHttpContextAccessor httpContextAccessor)
    {
        User = httpContextAccessor.HttpContext.User;
    }
}
#endif 