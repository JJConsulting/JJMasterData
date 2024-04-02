#if NET
using System;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace JJMasterData.Core.Http.AspNetCore;

internal class HttpRequestWrapper(
    IHttpContextAccessor httpContextAccessor,
    IFormValues formValues,
    IQueryString queryString)
    : IHttpRequest
{
    public IQueryString QueryString { get; } = queryString;
    public IFormValues Form { get; } = formValues;

    private HttpRequest Request => httpContextAccessor.HttpContext?.Request;

    private HttpContext HttpContext => httpContextAccessor.HttpContext;

    public string ApplicationUri => new Uri($"{Request.Scheme}://{Request.Host}{Request.PathBase}").ToString();
    public string this[string key] => GetValue(key);

    public string UserHostAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string HttpMethod => Request?.Method;

    public string UserAgent => Request?.Headers.UserAgent;

    public string AbsoluteUri => Request?.GetDisplayUrl();

    public string ApplicationPath => Request?.PathBase;

    public string ContentType => Request?.ContentType;
    public string Path => Request?.Path;

    private string GetValue(string key)
    {
        if (Request?.Query.ContainsKey(key) == true)
            return Request.Query[key];

        if (Request?.HasFormContentType == true)
            return Request.Form[key];

        return null;
    }
}

#endif