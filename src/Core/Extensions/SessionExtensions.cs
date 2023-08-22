#if NET || NETSTANDARD
using JJMasterData.Commons.Util;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace JJMasterData.Core.Extensions;

public static class SessionExtensions
{
    public static void SetObject(this ISession session, string key, object value)
    {
        session.SetString(key, JsonConvert.SerializeObject(value, new MemoryStreamJsonConverter()));
    }

    public static T GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonConvert.DeserializeObject<T>(value, new MemoryStreamJsonConverter());
    }
}
#endif