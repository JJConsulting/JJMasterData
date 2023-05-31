#nullable enable

using System.Collections;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Extensions;

public static class DictionaryExtensions
{
    public static T? ToModel<T>(this IDictionary dictionary, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        var serialized = JsonConvert.SerializeObject(dictionary, jsonSerializerSettings);
        return JsonConvert.DeserializeObject<T>(serialized, jsonSerializerSettings);
    }
}
