#nullable enable
using System;
using JJMasterData.Core.DataDictionary.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JJMasterData.Core.DataDictionary;

public static class FormElementSerializer
{
    private static JsonSerializerSettings? _settings;

    public static JsonSerializerSettings Settings =>
        _settings ??= new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
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