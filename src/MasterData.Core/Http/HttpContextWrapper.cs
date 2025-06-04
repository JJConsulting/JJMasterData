using System.Security.Claims;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http;

internal sealed class HttpContextWrapper(IHttpSession session, IHttpRequest request,
        IClaimsPrincipalAccessor claimsPrincipalAccessor)
    : IHttpContext
{
    public IHttpSession Session { get; } = session;
    public IHttpRequest Request { get; } = request;
    public ClaimsPrincipal User { get; } = claimsPrincipalAccessor.User;
}
