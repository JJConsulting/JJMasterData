#nullable enable


using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataManager.Models;

/// <summary>
/// Class used to send DataItem values to the client.
/// </summary>
public class DataItemResult
{
    [JsonPropertyName("id")]
    public required string Id { get; init; } 

    [JsonPropertyName("description")]
    public required string? Description { get; init; } 
    
    [JsonPropertyName("icon")] 
    public required string? IconCssClass { get; init; } 
    
    [JsonPropertyName("iconColor")] 
    public required string? IconColor { get; init; } 
}