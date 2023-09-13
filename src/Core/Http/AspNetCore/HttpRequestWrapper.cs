#if NET || NETSTANDARD

using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;

namespace JJMasterData.Core.Http.AspNetCore;

public class HttpRequestWrapper : IHttpRequest
{
    public IQueryString QueryString { get; }
    public string UserHostAddress { get; }
    public string HttpMethod { get; }
    public string UserAgent { get; }
    public string AbsoluteUri { get; }
    public string ApplicationPath { get; }
    public bool IsPost { get; }
    
    private HttpRequest Request { get; }

    public HttpRequestWrapper(IHttpContextAccessor httpContextAccessor, IQueryString queryString)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var request = httpContext.Request;
        var connection = httpContext.Connection;
        
        Request = httpContext.Request;
        QueryString = queryString;
        UserHostAddress = connection.RemoteIpAddress?.ToString();
        HttpMethod = request.Method;
        UserAgent = request.Headers["User-Agent"];
        AbsoluteUri = request.GetDisplayUrl();
        ApplicationPath = request.PathBase;
        IsPost = HttpMethod.Equals("POST");
    }

    public IFormFile GetFile(string file) => Request.Form.Files[file];

    public object GetUnvalidated(string key) => GetFormValue(key);

    public string this[string key] => GetValue(key);

    public string GetFormValue(string key)
    {
        if (Request.HasFormContentType)
            return Request.Form[key];

        return null;
    }

    private string GetValue(string key)
    {
        if (Request.Query.ContainsKey(key))
        {
            return Request.Query[key];
        }

        if (Request.HasFormContentType)
        {
            return Request.Form[key];
        }

        return null;
    }
}


#endif  