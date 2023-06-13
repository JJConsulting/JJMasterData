using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JJMasterData.WebApi.Models;


public class DicSyncInfoElement
{
    /// <summary>
    /// Dicionary Name
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Count command return
    /// </summary>
    [JsonProperty("recordSize")]
    public int RecordSize { get; set; }

    /// <summary>
    /// Executing time in milliseconds
    /// </summary>
    [JsonProperty("processMilliseconds")]
    public double ProcessMilliseconds { get; set; }
}