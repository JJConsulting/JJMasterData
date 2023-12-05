using Newtonsoft.Json;

namespace JJMasterData.Commons.Util;

public static class ObjectCloner
{
    /// <summary>
    /// Creates a deep copy of the object, removing the original reference.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="source">The object to be deep copied.</param>
    /// <returns>A new instance of the object with no reference to the original.</returns>
    public static T DeepCopy<T>(T source)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
        };
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source,settings),settings);
    }
}
