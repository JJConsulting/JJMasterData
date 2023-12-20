#if NETFRAMEWORK

using System;
using System.Web;
using System.Web.Mvc;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http.SystemWeb;

internal class SystemWebHttpRequestWrapper(IQueryString queryString, IFormValues form) : IHttpRequest
{
    private static HttpRequest Request => HttpContext.Current.Request;
    public string UserHostAddress => Request.UserHostAddress;
    public string HttpMethod=>  Request.HttpMethod;
    public string UserAgent =>  Request.UserAgent;
    public string AbsoluteUri => Request.Url.AbsoluteUri;
    public string ApplicationPath=>  Request.ApplicationPath;

    public string ApplicationUri
    {
        get
        {
            var request = HttpContext.Current.Request;
            var uri = new Uri($"{request.Url.Scheme}://{request.Url.Authority}{new UrlHelper(request.RequestContext).Content("~")}");

            if(!request.IsLocal)
                return uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port,
                    UriFormat.UriEscaped);

            return uri.ToString();
        }
    }
    public IFormValues Form  => form;
    public IQueryString QueryString { get; } = queryString;
    public string ContentType => Request.ContentType;

    public string this[string key] => Request[key];
}
#endif