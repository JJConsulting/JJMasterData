#if NETFRAMEWORK

using System.Web;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Http.SystemWeb;

internal class SystemWebHttpRequestWrapper : IHttpRequest
{
    private HttpRequest Request { get; } = HttpContext.Current.Request;
    public string UserHostAddress =>  Request.UserHostAddress;
    public string HttpMethod =>  Request.HttpMethod;
    public string UserAgent  =>  Request.UserAgent;
    public string AbsoluteUri  =>  Request.Url.AbsoluteUri;
    public string ApplicationPath => Request.ApplicationPath;
    public HttpPostedFile GetFile(string file) => Request.Files[file];
    public object GetUnvalidated(string key) => Request.Unvalidated[key];

    public string this[string key] =>Request[key];
    public IQueryString QueryString { get; }

    public string GetFormValue(string key) => Request.Form.Get(key);

    public bool IsPost => Request.HttpMethod.Equals("POST");
}
#endif