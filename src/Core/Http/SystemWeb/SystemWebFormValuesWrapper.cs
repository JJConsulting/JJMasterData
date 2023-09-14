using System.Collections.Specialized;
using System.Web;
using JJMasterData.Core.Http.Abstractions;

#if NET48

namespace JJMasterData.Core.Http.SystemWeb;

public class SystemWebFormValuesWrapper : IFormValues
{
    private UnvalidatedRequestValues UnvalidatedFormCollection { get; }
    private NameValueCollection FormCollection { get; }
    private HttpContext HttpContext { get; }

    public SystemWebFormValuesWrapper()
    {
        HttpContext = HttpContext.Current;
        FormCollection = HttpContext.Request.Form;
        UnvalidatedFormCollection = HttpContext.Request.Unvalidated;
    }
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