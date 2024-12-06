using System.Text.Json;

namespace JJMasterData.Commons.Serialization;

public static class MasterDataJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new DictionaryStringObjectJsonConverter() 
        }
    };
}