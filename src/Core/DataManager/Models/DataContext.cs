#nullable enable

using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.DataManager;

public class DataContext
{
    public DataContextSource Source { get; private set; }

    public string? UserId { get; private set; }

    public string? IpAddress { get; internal set; }
    
    public string? BrowserInfo { get; internal set; }

    public DataContext(IHttpContext httpContext,DataContextSource source, string? userId)
    {
        Source = source;
        UserId = userId;
        IpAddress = httpContext.Request.UserHostAddress;
        BrowserInfo = httpContext.Request.UserAgent;
    }

}