using System.Security.Claims;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http.SystemWeb;

public class SystemWebClaimsPrincipalWrapper : IClaimsPrincipalAccessor
{
    public ClaimsPrincipal User { get; }
}