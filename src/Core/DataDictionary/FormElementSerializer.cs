#nullable enable
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Repository;

public static class FormElementSerializer
{
    private static JsonSerializerSettings Settings { get; } = new()
    {
        TypeNameHandling = TypeNameHandling.Auto, 
        Formatting = Formatting.None,
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };
    
    public static string Serialize(FormElement formElement, Action<JsonSerializerSettings>? configureSettings = null)
    {
        configureSettings?.Invoke(Settings);
        return JsonConvert.SerializeObject(formElement, Settings);
    }

    public static FormElement Deserialize(string json, Action<JsonSerializerSettings>? configureSettings = null)
    {
        configureSettings?.Invoke(Settings);
        return JsonConvert.DeserializeObject<FormElement>(json, Settings)!;
    }
}