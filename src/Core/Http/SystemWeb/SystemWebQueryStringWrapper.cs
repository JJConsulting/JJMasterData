using System.Collections.Specialized;
using System.Web;
using JJMasterData.Core.Web.Http.Abstractions;

#if NETFRAMEWORK
namespace JJMasterData.Core.Http.SystemWeb;

public class SystemWebQueryStringWrapper : IQueryString
{
    private NameValueCollection QueryString { get; } = HttpContext.Current.Request.QueryString;
    public string this[string key] => QueryString[key];
    public string Value => QueryString.ToString();
}
#endif