#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Extensions;

public static class DictionaryExtensions
{
    public static T? ToModel<T>(this Dictionary<string, object> dictionary, JsonSerializerSettings? jsonSerializerSettings = null)
    {
        var serialized = JsonConvert.SerializeObject(dictionary, jsonSerializerSettings);
        return JsonConvert.DeserializeObject<T>(serialized, jsonSerializerSettings);
    }
}
