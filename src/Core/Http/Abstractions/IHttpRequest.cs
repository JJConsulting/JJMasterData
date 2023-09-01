using System.Web;


#if NET || NETSTANDARD
using Microsoft.AspNetCore.Http;
#endif

namespace JJMasterData.Core.Web.Http.Abstractions;

public interface IHttpRequest
{
    string UserHostAddress { get; }
    string HttpMethod { get; }
    string UserAgent { get; }
    string AbsoluteUri { get; }
    string ApplicationPath { get; }
#if NETFRAMEWORK
    HttpPostedFile GetFile(string file);
#else
    IFormFile GetFile(string file);
#endif
    object GetUnvalidated(string key);
    string this[string key] { get; }
    string QueryString(string key);
    string GetQueryString();
    string GetFormValue(string key);
    
    bool IsPost { get; }
}