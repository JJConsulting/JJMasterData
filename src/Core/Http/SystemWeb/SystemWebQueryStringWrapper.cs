using System.Collections.Specialized;
using System.Web;
using JJMasterData.Core.Http.Abstractions;

#if NETFRAMEWORK
namespace JJMasterData.Core.Http.SystemWeb;

internal class SystemWebQueryStringWrapper : IQueryString
{
    private NameValueCollection QueryString { get; } = HttpContext.Current.Request.QueryString;
    public string this[string key] => QueryString[key];
    public string Value => QueryString.ToString();
}
#endif