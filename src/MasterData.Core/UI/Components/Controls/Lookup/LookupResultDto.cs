#nullable enable


using System.Text.Json.Serialization;

namespace JJMasterData.Core.UI.Components;

public record LookupResultDto(string Id,string Description)
{
    [JsonPropertyName("id")]
    public string Id { get; } = Id;
    [JsonPropertyName("description")] public string? Description { get; } = Description;
}