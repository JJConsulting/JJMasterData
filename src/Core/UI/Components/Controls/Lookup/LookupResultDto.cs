#nullable enable
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

public record LookupResultDto(string Id,string Description)
{
    [JsonProperty("id")]
    public string Id { get; } = Id;
    [JsonProperty("description")] public string? Description { get; } = Description;
}