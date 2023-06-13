using System.Collections;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Extensions;

public static class DictionaryExtensions
{
    public static T ToModel<T>(this IDictionary hashtable)
    {
        var serialized = JsonConvert.SerializeObject(hashtable);
        return JsonConvert.DeserializeObject<T>(serialized);
    }
}
