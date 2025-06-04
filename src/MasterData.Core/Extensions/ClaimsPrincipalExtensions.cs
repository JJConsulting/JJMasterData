#nullable enable
using System.Security.Claims;

namespace JJMasterData.Core.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        string? userId = null;
        foreach (var claim in claimsPrincipal.Claims)
        {
            if (claim.Type == ClaimTypes.NameIdentifier)
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