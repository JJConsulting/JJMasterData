using JJMasterData.Core.Http;

namespace JJMasterData.Core.DataManager.AuditLog;

public class AuditLogData
{
    public string IpAddress { get; set; }
    public string UserId { get; set; }
    public string BrowserInfo { get; set; }
    public AuditLogSource Source { get; set; }

    public AuditLogData(AuditLogSource source)
    {
        var context = JJHttpContext.GetInstance();
        Source = source;
        IpAddress = context.Request.UserHostAddress;
        BrowserInfo = context.Request.UserAgent;
        UserId = context.Session?["USERID"];
    }

}