using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public record LookupUrlDto(string Url)
{
    [JsonProperty("url")]
    public string Url { get; } = Url;
    public string ToJson() => JsonConvert.SerializeObject(this);
}