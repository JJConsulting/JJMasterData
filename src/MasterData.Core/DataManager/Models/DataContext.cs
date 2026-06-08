#nullable enable

using Microsoft.AspNetCore.Http;
using JJMasterData.Core.Extensions;

namespace JJMasterData.Core.DataManager.Models;

public class DataContext
{
    public DataContextSource Source { get; }

    public string? UserId { get; }

    public string? IpAddress { get; }

    public string? BrowserInfo { get; }

    public DataContext()
    {
    }

    public DataContext(IHttpContextAccessor request, DataContextSource source, string? userId)
        : this(request.HttpContext!.Request, source, userId)
    {
    }

    public DataContext(HttpRequest request, DataContextSource source, string? userId)
    {
        Source = source;
        UserId = userId;
        IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString();
        BrowserInfo = request.Headers.UserAgent.ToString();
    }
}
