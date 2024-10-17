#nullable enable
using System.Linq;
using System.Security.Claims;

namespace JJMasterData.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.Claims.FirstOrDefault(c=>c.Type == ClaimTypes.NameIdentifier)?.Value;
    }
}