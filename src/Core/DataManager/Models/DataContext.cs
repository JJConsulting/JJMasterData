#nullable enable

using JJMasterData.Core.Http.Abstractions;

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

    public DataContext(IHttpRequest request, DataContextSource source, string? userId)
    {
        Source = source;
        UserId = userId;
        IpAddress = request.UserHostAddress;
        BrowserInfo = request.UserAgent;
    }
}