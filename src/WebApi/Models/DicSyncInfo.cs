using Newtonsoft.Json;

namespace JJMasterData.WebApi.Models;


public class DicSyncInfo
{
    /// <summary>
    /// Lista de dicionários com retorno do count
    /// </summary>
    [JsonProperty("listElement")]
    public List<DicSyncInfoElement> ListElement { get; set; } = new();

    /// <summary>
    /// Server date.
    /// Format "yyyy-MM-dd HH:mm"
    /// </summary>
    [JsonProperty("serverDate")]
    public string? ServerDate { get; set; }

    /// <summary>
    /// Total processing time in milliseconds
    /// </summary>
    [JsonProperty("totalProcessMilliseconds")]
    public double TotalProcessMilliseconds { get; set; }
}