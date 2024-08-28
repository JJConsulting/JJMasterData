#if NET
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

internal sealed class HttpSessionWrapper(IHttpContextAccessor contextAccessor) : IHttpSession
{
    private ISession Session { get; } = contextAccessor.HttpContext.Session;

    public string this[string key]
    {
        get => Session.GetObject<string>(key);
        set => Session.SetObject(key,value);
    }
    public void SetSessionValue(string key, object value) => Session.SetObject(key, value);

    public T GetSessionValue<T>(string key) => Session.GetObject<T>(key);
    public bool HasSession() => Session != null;
}
#endif  