#if NETFRAMEWORK

using System.Web;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Http.SystemWeb;

internal class SystemWebHttpRequestWrapper : IHttpRequest
{
    private HttpRequest Request { get; }
    public string UserHostAddress { get; }
    public string HttpMethod { get; }
    public string UserAgent { get; }
    public string AbsoluteUri { get; }
    public string ApplicationPath { get; }
    public IFormValues Form { get; }
    public bool IsPost { get; }
    public IQueryString QueryString { get; }

    public SystemWebHttpRequestWrapper(HttpRequest request, IQueryString queryString, IFormValues form)
    {
        Request = request;
        QueryString = queryString;
        Form = form;
        UserHostAddress = Request.UserHostAddress;
        HttpMethod = Request.HttpMethod;
        UserAgent = Request.UserAgent;
        AbsoluteUri = Request.Url.AbsoluteUri;
        ApplicationPath = Request.ApplicationPath;
        IsPost = Request.HttpMethod.Equals("POST");
    }

    public HttpPostedFile GetFile(string file) => Request.Files[file];
    public object GetUnvalidated(string key) => Request.Unvalidated[key];

    public string this[string key] => Request[key];
    public string GetFormValue(string key) => Request.Form.Get(key);
}
#endif