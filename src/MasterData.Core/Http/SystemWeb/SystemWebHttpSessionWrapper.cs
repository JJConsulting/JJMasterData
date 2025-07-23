#if NETFRAMEWORK
using System;
using System.Web;
using System.Web.SessionState;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http.SystemWeb;

internal sealed class SystemWebHttpSessionWrapper : IHttpSession
{
    private static HttpSessionState Session => HttpContext.Current.Session;
    public string this[string key]
    {
        get => Session[key]?.ToString();
        set => Session[key] = value;
    }

    public void SetSessionValue(string key, object value) => Session[key] = value;

    public T GetSessionValue<T>(string key) => (T)Session[key];
    public bool HasKey(string key)
    {
        foreach (var existingKey in Session.Keys)
        {
            if(string.IsNullOrEmpty(existingKey?.ToString()))
                continue;
            
            if(string.Equals(existingKey.ToString(), key))
                return true;
        }

        return false;
    }

    public bool HasSession()
    {
        try
        {
            return Session != null;
        }
        catch 
        {
            return false;
        }
    }
}
#endif