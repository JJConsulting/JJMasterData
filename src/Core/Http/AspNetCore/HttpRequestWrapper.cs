#if NET || NETSTANDARD

using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;

namespace JJMasterData.Core.Http.AspNetCore;

public class HttpRequestWrapper : IHttpRequest
{
    public IQueryString QueryString { get; }
    private ConnectionInfo Connection { get; }
    private HttpRequest Request { get; }
    public string UserHostAddress => Connection.RemoteIpAddress?.ToString();
    public string HttpMethod => Request.Method;
    public string UserAgent => Request.Headers["User-Agent"];
    public string AbsoluteUri => Request.GetDisplayUrl();
    public string ApplicationPath => Request.PathBase;

    public HttpRequestWrapper(IHttpContextAccessor httpContextAccessor, IQueryString queryString)
    {
        QueryString = queryString;
        var httpContext = httpContextAccessor.HttpContext;
        Request = httpContext.Request;
        Connection = httpContext.Connection;
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

    public bool IsPost => HttpMethod.Equals("POST");

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