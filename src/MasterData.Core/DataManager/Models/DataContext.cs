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

    public DataContext(DataContextSource source, string? userId, string? ipAddress = null, string? browserInfo = null)
    {
        Source = source;
        UserId = userId;
        IpAddress = ipAddress;
        BrowserInfo = browserInfo;
    }
}
