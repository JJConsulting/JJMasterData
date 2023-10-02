using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http;

internal class HttpContextWrapper : IHttpContext
{
    public HttpContextWrapper(IHttpSession session, IHttpRequest request)
    {
        Session = session;
        Request = request;
    }

    public IHttpSession Session { get; }
    public IHttpRequest Request { get; }
}
