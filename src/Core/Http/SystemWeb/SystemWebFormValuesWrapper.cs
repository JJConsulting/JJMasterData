using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using JJMasterData.Core.Http.Abstractions;

#if NET48

namespace JJMasterData.Core.Http.SystemWeb;

internal class SystemWebFormValuesWrapper : IFormValues
{
    private Dictionary<string,string> FormValues { get; }
    private Dictionary<string,string> UnvalidatedFormValues { get; }
    private static UnvalidatedRequestValues UnvalidatedFormCollection => HttpContext.Request.Unvalidated;
    private static NameValueCollection FormCollection => HttpContext.Request.Form;
    private static HttpContext HttpContext => HttpContext.Current;
    
    public bool ContainsFormValues() => FormCollection is { Count: > 0 };
    public string this[string key] => FormValues.TryGetValue(key, out var value) ? value : null;

    public HttpPostedFile GetFile(string file) => HttpContext?.Request.Files[file];
    public string GetUnvalidated(string key) => UnvalidatedFormValues.TryGetValue(key, out var value) ? value : null;

    public SystemWebFormValuesWrapper()
    {
        FormValues = new Dictionary<string, string>();
        UnvalidatedFormValues = new Dictionary<string, string>();
        
        if (HttpContext.Current == null)
            return;
        
        try
        {
            _ = HttpContext.Current.Request;
        }
        catch
        {
            return;
        }
        
        if (!ContainsFormValues())
            return;
        
        foreach (var key in FormCollection.Keys)
        {
            FormValues[key.ToString()] = FormCollection[key.ToString()];
        }
        foreach (var key in UnvalidatedFormCollection.Form.Keys)
        {
            UnvalidatedFormValues[key.ToString()] = UnvalidatedFormCollection[key.ToString()];
        }
    }
}
#endif