#if NET

using System.Text.Json;
using JJMasterData.Commons.Serialization;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.Extensions;

public static class SessionExtensions
{
    public static void SetObject(this ISession session, string key, object value)
    {
        session.SetString(key, JsonSerializer.Serialize(value, MasterDataJsonSerializerOptions.Default));
    }

    public static T GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value, MasterDataJsonSerializerOptions.Default);
    }
}
#endif