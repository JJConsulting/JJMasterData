using System.Collections.Specialized;
using System.Web;
using JJMasterData.Core.Http.Abstractions;

#if NETFRAMEWORK
namespace JJMasterData.Core.Http.SystemWeb;

internal sealed class SystemWebQueryStringWrapper : IQueryString
{
    private static NameValueCollection QueryString  => HttpContext.Current.Request.QueryString;
    public string this[string key] => QueryString[key];
    public string Value => QueryString.ToString();
}
#endif