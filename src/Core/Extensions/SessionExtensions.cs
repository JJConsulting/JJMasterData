#if NET

using System.Text.Json;
using JJMasterData.Core.Serialization;
using Microsoft.AspNetCore.Http;


namespace JJMasterData.Core.Extensions;

public static class SessionExtensions
{
    public static void SetObject(this ISession session, string key, object value)
    {
        session.SetString(key, JsonSerializer.Serialize(value, SerializerOptions.Default));
    }

    public static T GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value, SerializerOptions.Default);
    }
}
#endif