#nullable enable

using JJMasterData.Core.Web.Http;

namespace JJMasterData.Core.DataManager;

public class DataContext
{
    public DataContextSource Source { get; private set; }

    public string? UserId { get; private set; }

    public string? IpAddress { get; internal set; }
    
    public string? BrowserInfo { get; internal set; }
    
    public DataContext(DataContextSource source, string? userId)
    {
        Source = source;
        UserId = userId;

        var context = JJHttpContext.GetInstance();
        if (context.HasContext())
        {
            IpAddress = context.Request.UserHostAddress;
            BrowserInfo = context.Request.UserAgent;
        }

    }

}