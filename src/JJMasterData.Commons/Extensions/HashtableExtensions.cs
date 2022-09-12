using System.Collections;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Extensions;

public static class HashtableExtensions
{
    public static T ToModel<T>(this Hashtable hashtable)
    {
        var serialized = JsonConvert.SerializeObject(hashtable);
        return JsonConvert.DeserializeObject<T>(serialized);
    }
}
