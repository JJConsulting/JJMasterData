#nullable enable
using System;
using JJMasterData.Core.DataDictionary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JJMasterData.Core.DataDictionary;

public static class FormElementSerializer
{
    public static readonly JsonSerializerSettings Settings;
    
    static FormElementSerializer()
    {
        Settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
    }
    
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