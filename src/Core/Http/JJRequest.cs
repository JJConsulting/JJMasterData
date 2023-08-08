using JJMasterData.Core.Web.Http.Abstractions;

#if NET || NETSTANDARD
using Microsoft.AspNetCore.Http.Extensions;
#endif

namespace JJMasterData.Core.Web.Http;

public class JJRequest : IHttpRequest
{

#if NET || NETSTANDARD
    private Microsoft.AspNetCore.Http.HttpContext HttpContext { get; }
    
    public JJRequest(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
    {
        HttpContext = httpContextAccessor.HttpContext;
    }
#endif

    public string UserHostAddress
    {
        get
        {
#if NETFRAMEWORK
            return System.Web.HttpContext.Current.Request.UserHostAddress;
#else
            return HttpContext.Connection.RemoteIpAddress?.ToString();
#endif
        }

    }

    public string HttpMethod
    {

        get
        {
#if NETFRAMEWORK
            return System.Web.HttpContext.Current.Request.HttpMethod;
#else
            return HttpContext.Request.Method;
#endif
        }

    }
    
    public string ApplicationPath
    {

        get
        {
#if NETFRAMEWORK
            return System.Web.HttpContext.Current.Request.ApplicationPath;
#else
            return HttpContext.Request.PathBase;
#endif
        }

    }
#if NETFRAMEWORK
    public System.Web.HttpPostedFile GetFile(string file)
    {
        return System.Web.HttpContext.Current.Request.Files[file];
    }
#else
    public Microsoft.AspNetCore.Http.IFormFile GetFile(string file) => HttpContext.Request.Form.Files[file];
#endif

    public object GetUnvalidated(string key)
    {
#if NETFRAMEWORK
        return System.Web.HttpContext.Current.Request.Unvalidated[key];
#else
        return GetValue(key);
#endif
    }

    public string UserAgent
    {
        get
        {
#if NETFRAMEWORK
            return System.Web.HttpContext.Current.Request.UserAgent;
#else

            return HttpContext.Request.Headers["User-Agent"];
#endif
        }
    }


#if NETFRAMEWORK
    public string AbsoluteUri => System.Web.HttpContext.Current.Request.Url.AbsoluteUri;
#else
    public string AbsoluteUri => HttpContext.Request.GetDisplayUrl();
#endif


    public string this[string key] => GetValue(key);


    public string GetValue(string key)
    {
#if NETFRAMEWORK
        return System.Web.HttpContext.Current.Request[key];
#else

        if (HttpContext.Request.Query.ContainsKey(key))
        {
            return HttpContext.Request.Query[key];
        }

        if (HttpContext.Request.HasFormContentType)
        {
            return HttpContext.Request.Form[key];
        }

        return null;
#endif
    }

    public string QueryString(string key)
    {
#if NETFRAMEWORK
        return System.Web.HttpContext.Current.Request.QueryString[key];
#else
        return HttpContext.Request.Query[key];
#endif
    }


    public string Form(string key)
    {
#if NETFRAMEWORK
        return System.Web.HttpContext.Current.Request.Form[key];
#else
        if (HttpContext.Request.HasFormContentType)
            return HttpContext.Request.Form[key];

        return null;
#endif
    }

}