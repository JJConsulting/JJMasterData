#if NET
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace JJMasterData.Core.Http.AspNetCore;

internal class HttpRequestWrapper : IHttpRequest
{
    public IQueryString QueryString { get; }
    public IFormValues Form { get; }
    public string UserHostAddress { get; }
    public string HttpMethod { get; }
    public string UserAgent { get; }
    public string AbsoluteUri { get; }
    public string ApplicationPath { get; }
    public bool IsPost { get; }
    private HttpRequest Request { get; }
    public HttpRequestWrapper(
        IHttpContextAccessor httpContextAccessor, 
        IFormValues formValues,
        IQueryString queryString)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var request = httpContext.Request;
        var connection = httpContext.Connection;
        
        Request = httpContext.Request;
        Form = formValues;
        QueryString = queryString;
        UserHostAddress = connection.RemoteIpAddress?.ToString();
        HttpMethod = request.Method;
        UserAgent = request.Headers["User-Agent"];
        AbsoluteUri = request.GetDisplayUrl();
        ApplicationPath = request.PathBase;
        IsPost = HttpMethod.Equals("POST");
    }

    public IFormFile GetFile(string file)
    {
        if (Request.HasFormContentType)
            return Request.Form.Files[file];
        return null;
    }

    public string this[string key] => GetValue(key);

    private string GetValue(string key)
    {
        if (Request.Query.ContainsKey(key))
            return Request.Query[key];

        if (Request.HasFormContentType)
            return Request.Form[key];
        
        return null;
    }
}

#endif  