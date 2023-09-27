using JJMasterData.Core.DataDictionary.Repository;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace JJMasterData.Web.Extensions;

public static class TempDataExtensions
{
    public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
    {
        tempData[key] = JsonConvert.SerializeObject(value, FormElementSerializer.Settings);
    }

    public static T? Get<T>(this ITempDataDictionary tempData, string key) where T : class
    {
        tempData.TryGetValue(key, out var value);

        return value == null ? null : JsonConvert.DeserializeObject<T>((string)value, FormElementSerializer.Settings);
    }
}