#if NET
using System.Security.Claims;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

public class ClaimsPrincipalWrapper(IHttpContextAccessor httpContextAccessor) : IClaimsPrincipalAccessor
{
    public ClaimsPrincipal User { get; } = httpContextAccessor.HttpContext.User;
}
#endif 