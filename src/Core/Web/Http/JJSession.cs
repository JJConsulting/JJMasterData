#if NET || NETSTANDARD
using Microsoft.AspNetCore.Http;
using JJMasterData.Core.Extensions;
#elif NETFRAMEWORK
using System.Web;
#endif

using JJMasterData.Core.Web.Http.Abstractions;


namespace JJMasterData.Core.Web.Http;

/// <summary>
/// Session helper class.
/// </summary>
public class JJSession : IHttpSession
{
    
#if NET || NETSTANDARD
    private HttpContext HttpContext { get; }
    
    public JJSession(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext = httpContextAccessor.HttpContext;
    }
#endif

    public string this[string key]
    {
        get
        {
#if NETFRAMEWORK
            return HttpContext.Current.Session[key]?.ToString();
#else
            return HttpContext.Session.GetString(key);
#endif
        }
        set
        {
#if NETFRAMEWORK
            HttpContext.Current.Session[key] = value;
#else
            HttpContext.Session.SetString(key, value ?? string.Empty);
#endif
        }
    }

    public void SetSessionValue(string key, object value)
    {
#if NETFRAMEWORK
        HttpContext.Current.Session[key] = value;
#else
        HttpContext?.Session.SetObject(key, value);
#endif
    }


    public T GetSessionValue<T>(string key)
    {
#if NETFRAMEWORK
        return (T)HttpContext.Current.Session[key] ?? default;
#else
        return HttpContext.Session.GetObject<T>(key);
#endif
    }
}