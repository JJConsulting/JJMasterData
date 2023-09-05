#if NET || NETSTANDARD
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

public class HttpSessionWrapper : IHttpSession
{
    private ISession Session { get; }
    public HttpSessionWrapper(IHttpContextAccessor contextAccessor)
    {
        Session = contextAccessor.HttpContext.Session;
    }
    public string this[string key]
    {
        get => Session.GetObject<string>(key);
        set => Session.SetObject(key,value);
    }
    public void SetSessionValue(string key, object value) => Session.SetObject(key, value);

    public T GetSessionValue<T>(string key) => Session.GetObject<T>(key);
}
#endif  