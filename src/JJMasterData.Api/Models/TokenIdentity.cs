using System.Security.Principal;

namespace JJMasterData.Api.Models;

public class TokenIdentity : IIdentity
{
    public string Name { get; internal set; }
    public string AuthenticationType { get; internal set; }
    public bool IsAuthenticated { get; internal set; }
}