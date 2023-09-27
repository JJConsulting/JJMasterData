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
}