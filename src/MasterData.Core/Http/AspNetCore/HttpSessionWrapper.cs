#if NET
using System;
using System.Linq;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Http.AspNetCore;

internal sealed class HttpSessionWrapper(IHttpContextAccessor contextAccessor) : IHttpSession
{
    private readonly ISession _session = contextAccessor.HttpContext.Session;

    public string this[string key]
    {
        get => _session.GetString(key);
        set
        {
            if(value == null)
                _session.Remove(key);
            else
                _session.SetString(key, value);
        }
    }

    public void SetSessionValue(string key, object value) => _session.SetObject(key, value);

    public T GetSessionValue<T>(string key) => _session.GetObject<T>(key);
    public bool HasKey(string key)
    {
        return _session.Keys.Any(k=>k == key);
    }

    public bool HasSession() => _session != null;
}
#endif  