#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif
using System.Threading;

namespace JJMasterData.Core.DataDictionary;

public class MasterDataUrlHelper
{
    public static string GetUrl(string url)
    {
        string value = url;

#if NETFRAMEWORK
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                    value = "/" + HttpContext.Current.Request.ApplicationPath.Replace("/", "");
            }
#else
        var context = new HttpContextAccessor().HttpContext;
        if (string.IsNullOrEmpty(value))
        {
            value = string.Empty;
            if (context != null && context.Request != null)
                value = "/" + context.Request.PathBase.ToString().Replace("/", "");
        }
#endif

        if (!value.EndsWith("/"))
            value += "/";

        value += Thread.CurrentThread.CurrentUICulture.Name;
        value += "/MasterData/";

        return value;
    }
}