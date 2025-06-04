

using System.Text.Json.Serialization;

namespace JJMasterData.WebApi.Models;


public class DicSyncInfo
{
    /// <summary>
    /// Lista de dicionários com retorno do count
    /// </summary>
    [JsonPropertyName("listElement")]
    public List<DicSyncInfoElement> ListElement { get; set; } = [];

    /// <summary>
    /// Server date.
    /// Format "yyyy-MM-dd HH:mm"
    /// </summary>
    [JsonPropertyName("serverDate")]
    public string? ServerDate { get; set; }

    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    [JsonPropertyName("totalProcessMilliseconds")]
    public double TotalProcessMilliseconds { get; set; }
}