using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Commons.Extensions;

public static class GenericExtensions
{
    /// <summary>
    /// Returns a new copy of the object, without the old reference.
    /// </summary>
    /// <param name="self"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T DeepCopy<T>(this T self)
    {
        return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(self));
    }
    
    public static IDictionary<string, string> ToDictionary(this object metaToken)
    {
        if (metaToken == null)
        {
            return null;
        }

        if (metaToken is not JToken token)
        {
            return ToDictionary(JObject.FromObject(metaToken));
        }

        if (token.HasValues)
        {
            var contentData = new Dictionary<string, string>();

            return token.Children()
                .ToList()
                .Select(child => child.ToDictionary())
                .Where(childContent => childContent != null)
                .Aggregate(contentData, (current, childContent) => current.Concat(childContent)
                    .ToDictionary(k => k.Key, v => v.Value));
        }

        var jValue = token as JValue;
        if (jValue?.Value == null)
        {
            return null;
        }

        var value = jValue?.Type == JTokenType.Date ?
            jValue?.ToString("o", CultureInfo.InvariantCulture) :
            jValue?.ToString(CultureInfo.InvariantCulture);

        return new Dictionary<string, string> { { token.Path, value } };
    }

}