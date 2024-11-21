using System.Text.Json;
using JJMasterData.Commons.Serialization;

namespace JJMasterData.Core.Serialization;

public static class SerializerOptions
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