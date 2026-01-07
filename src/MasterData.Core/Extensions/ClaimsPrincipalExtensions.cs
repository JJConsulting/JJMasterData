#nullable enable
using System.Security.Claims;

namespace JJMasterData.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal claimsPrincipal, string userIdClaimName)
    {
        string? userId = null;
        foreach (var claim in claimsPrincipal.Claims)
        {
            if (claim.Type == userIdClaimName)
            {
                userId = claim.Value;
                break;
            }
        }
        
        if(string.IsNullOrEmpty(userId))
            return claimsPrincipal.Identity?.Name;

        return userId;
    }
}