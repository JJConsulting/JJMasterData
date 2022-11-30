using System.Security.Claims;
using System.Security.Principal;

namespace JJMasterData.Api.Models;

public class TokenPrincipal : ClaimsPrincipal
{
    public new IIdentity? Identity { get; internal set; }
    public TokenInfo? TokenInfo { get; internal set; }

    public new bool IsInRole(string role)
    {
        return !string.IsNullOrEmpty(role) && role.Contains("api");
    }
    
    public TokenPrincipal(TokenIdentity? identity)
    {
        Identity = identity;
    }
}