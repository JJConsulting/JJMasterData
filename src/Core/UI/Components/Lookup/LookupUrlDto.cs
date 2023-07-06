using Newtonsoft.Json;

namespace JJMasterData.Core.Web.Components;

public record LookupUrlDto(string Url)
{
    [JsonProperty("url")]
    public string Url { get; } = Url;
    public string ToJson() => JsonConvert.SerializeObject(this);
}