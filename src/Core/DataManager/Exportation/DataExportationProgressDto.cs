using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Exportation;

internal record DataExportationProgressDto
{
    [JsonProperty(nameof(StartDate))]
    public string StartDate { get; set; }

    [JsonProperty(nameof(IsProcessing))]
    public bool IsProcessing { get; set; }

    [JsonProperty(nameof(Message))]
    public string Message { get; set; }

    [JsonProperty(nameof(FinishedMessage))]
    public string FinishedMessage { get; set; }

    [JsonProperty(nameof(HasError))]
    public bool HasError { get; set; }

    [JsonProperty(nameof(PercentProcess))]
    public int PercentProcess { get; set; }
}
