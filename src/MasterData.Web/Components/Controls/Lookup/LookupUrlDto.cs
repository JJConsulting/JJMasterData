

using System.Text.Json;
using System.Text.Json.Serialization;

namespace JJMasterData.Web.Components;

public class LookupUrlDto(string Url)
{
    [JsonPropertyName("url")]
    public string Url { get; } = Url;
    public string ToJson() => JsonSerializer.Serialize(this);
}