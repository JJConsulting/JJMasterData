// ReSharper disable RedundantUsingDirective
#if NETFRAMEWORK
using System.Web;
using System.Windows.Input;
#endif
using JJMasterData.Core.Extensions;


#if NET || NETSTANDARD
using Microsoft.AspNetCore.Http;
#endif
// ReSharper disable RedundantNameQualifier

namespace JJMasterData.Core.Web.Http;

/// <summary>
/// Session helper class.
/// </summary>
public class JJSession
{
#if NETFRAMEWORK
    private static System.Web.HttpContext SystemWebCurrent => JJHttpContext.SystemWebCurrent;
#else
    private static Microsoft.AspNetCore.Http.HttpContext AspNetCoreCurrent => JJHttpContext.AspNetCoreCurrent;
#endif

    public bool HasSession()
    {
#if NETFRAMEWORK
        return SystemWebCurrent.Session != null;

#else
            return AspNetCoreCurrent.Session != null;
#endif
    }

    public string this[string key]
    {
        get
        {
#if NETFRAMEWORK
            object obj = SystemWebCurrent.Session[key];
            return obj == null ? null : SystemWebCurrent.Session[key].ToString();

#else
            return AspNetCoreCurrent.Session.GetString(key);
#endif
        }
        set
        {
#if NETFRAMEWORK
            SystemWebCurrent.Session[key] = value;
#else
            AspNetCoreCurrent.Session.SetString(key, value ?? string.Empty);
#endif
        }
    }

    internal static void SetSessionValue(string key, object value)
    {
#if NETFRAMEWORK
        SystemWebCurrent.Session[key] = value;
#else
        AspNetCoreCurrent?.Session.SetObject(key, value);
#endif
    }


    internal static T GetSessionValue<T>(string key)
    {
#if NETFRAMEWORK
        return (T)SystemWebCurrent.Session[key] ?? default;
#else
        return AspNetCoreCurrent.Session.GetObject<T>(key);
#endif
    }
}