#if NETFRAMEWORK
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http.SystemWeb;

internal class SystemWebHttpSessionWrapper : IHttpSession
{
    private static HttpSessionState Session => HttpContext.Current.Session;
    private Dictionary<string,object> SessionValues { get; }
    
    public string this[string key]
    {
        get => SessionValues.TryGetValue(key, out var value) ? value.ToString() : null;
        set => Session[key] = value;
    }

    public void SetSessionValue(string key, object value) => Session[key] = value;

    public T GetSessionValue<T>(string key)
    {
        if (SessionValues.TryGetValue(key, out var value))
        {
            return (T)SessionValues[key];
        }

        return default;
    }

    public SystemWebHttpSessionWrapper()
    {
        SessionValues = new Dictionary<string, object>();
        
        try
        {
            _ = HttpContext.Current.Session;
        }
        catch
        {
            return;
        }
        
        if (HttpContext.Current.Session == null)
            return;

        foreach (var key in Session.Keys)
        {
            SessionValues[key.ToString()] = Session[key.ToString()];
        }
    }
}
#endif