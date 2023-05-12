using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Serializers;

public static class FormElementSerializer
{
    private static JsonSerializerSettings Settings { get; } = new()
    {
        TypeNameHandling = TypeNameHandling.Auto, 
        Formatting = Formatting.Indented,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };
    
    public static string Serialize(FormElement formElement)
    {
        return JsonConvert.SerializeObject(formElement, Settings);
    }

    public static FormElement Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<FormElement>(json, Settings);
    }
}