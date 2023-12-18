#if NETFRAMEWORK

using System.Web;
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
    public IFormValues Form  => form;
    public bool IsPost => Request.HttpMethod.Equals("POST");
    public IQueryString QueryString { get; } = queryString;
    public string ContentType => Request.ContentType;
    public HttpPostedFile GetFile(string file) => Request.Files[file];
    public object GetUnvalidated(string key) => Request.Unvalidated[key];

    public string this[string key] => Request[key];
    public string GetFormValue(string key) => Request.Form.Get(key);
}
#endif