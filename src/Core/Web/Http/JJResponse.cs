#if NETSTANDARD || NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Web;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Http;

public class JJResponse : IHttpResponse
{
    
#if NET || NETSTANDARD
    private Microsoft.AspNetCore.Http.HttpContext HttpContext { get; }
    
    public JJResponse(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext = httpContextAccessor.HttpContext;
    }
#endif
    

    /// <inheritdoc cref="SendResponse"/>
    public void SendResponse(string data, string contentType = null)
    {
        System.Web.HttpContext.Current!.Response.ClearContent();
#if NETFRAMEWORK
        if (contentType != null)
        {
            HttpContext.Current.Response.ContentType = contentType;
        }
#else
        if (!HttpContext.Response.HasStarted && contentType != null)
        {
            HttpContext.Response.ContentType = contentType;
        }
#endif
        System.Web.HttpContext.Current!.Response.Write(data);
        System.Web.HttpContext.Current!.Response.End();
    }

    public void ClearResponse()
    {
#if NETFRAMEWORK
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.ClearHeaders();
        HttpContext.Current.Response.ClearContent();
#else
        HttpContext.Response.Clear();
        HttpContext.Response.Headers.Clear();
#endif
    }

    public void AddResponseHeader(string key, string value)
    {
#if NETFRAMEWORK
        HttpContext.Current.Response.Headers.Add(key, value);
#else
        if(!HttpContext.Response.HasStarted)
            HttpContext.Response.Headers.Add(key, value);
#endif
    }


#if NETFRAMEWORK
    public void Redirect(string url) => HttpContext.Current.Response.Redirect(url);
#else
    public void Redirect(string url) => HttpContext.Response.Redirect(url);
#endif

}