using System.Security.Claims;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http;

internal class HttpContextWrapper : IHttpContext
{
    public IHttpSession Session { get; }
    public IHttpRequest Request { get; }
    public ClaimsPrincipal User { get; }
    public HttpContextWrapper(IHttpSession session, IHttpRequest request, IClaimsPrincipalAccessor claimsPrincipalAccessor)
    {
        Session = session;
        Request = request;
        User = claimsPrincipalAccessor.User;
    }
}
