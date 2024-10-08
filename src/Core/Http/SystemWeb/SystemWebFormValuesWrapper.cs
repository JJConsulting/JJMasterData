using System.Collections.Specialized;
using System.Web;
using JJMasterData.Core.Http.Abstractions;

#if NET48

namespace JJMasterData.Core.Http.SystemWeb;

internal sealed class SystemWebFormValuesWrapper : IFormValues
{
    private static UnvalidatedRequestValues UnvalidatedFormCollection => HttpContext.Request.Unvalidated;
    private static NameValueCollection FormCollection => HttpContext.Request.Form;
    private static HttpContext HttpContext => HttpContext.Current;
    
    public bool ContainsFormValues() => FormCollection is { Count: > 0 };
    public string this[string key] => FormCollection?[key];
    public HttpPostedFile GetFile(string file)
    {
        return HttpContext?.Request.Files[file];
    }
    public string GetUnvalidated(string key)
    {
        return UnvalidatedFormCollection[key];
    }
}
#endif