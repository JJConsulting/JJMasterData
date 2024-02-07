#nullable enable

using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Models;

/// <summary>
/// Class used to send DataItem values to the client.
/// </summary>
public class DataItemResult
{
    [JsonProperty("id")]
    public required string Id { get; init; } 

    [JsonProperty("description")]
    public required string? Description { get; init; } 
    
    [JsonProperty("icon")] 
    public required string? IconCssClass { get; init; } 
    
    [JsonProperty("iconColor")] 
    public required string? IconColor { get; init; } 
}