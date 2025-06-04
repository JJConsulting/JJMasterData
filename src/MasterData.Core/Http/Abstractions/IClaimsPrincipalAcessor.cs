using System.Security.Claims;

namespace JJMasterData.Core.Http.Abstractions;

public interface IClaimsPrincipalAccessor
{
    ClaimsPrincipal User { get; }
}