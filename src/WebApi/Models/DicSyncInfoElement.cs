

using System.Text.Json.Serialization;

namespace JJMasterData.WebApi.Models;


public class DicSyncInfoElement
{
    /// <summary>
    /// Dicionary Name
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Count command return
    /// </summary>
    [JsonPropertyName("recordSize")]
    public int RecordSize { get; set; }

    /// <summary>
    /// Executing time in milliseconds
    /// </summary>
    [JsonPropertyName("processMilliseconds")]
    public double ProcessMilliseconds { get; set; }
}