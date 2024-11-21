

using System.Text.Json;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.UI.Components;

public class LookupUrlDto(string Url)
{
    [JsonPropertyName("url")]
    public string Url { get; } = Url;
    public string ToJson() => JsonSerializer.Serialize(this);
}