using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Exports;

internal record DataExpDto
{
    [JsonProperty("StartDate")]
    public string StartDate { get; set; }

    [JsonProperty("IsProcessing")]
    public bool IsProcessing { get; set; }

    [JsonProperty("Message")]
    public string Message { get; set; }

    [JsonProperty("FinishedMessage")]
    public string FinishedMessage { get; set; }

    [JsonProperty("HasError")]
    public bool HasError { get; set; }

    [JsonProperty("PercentProcess")]
    public int PercentProcess { get; set; }
}
