

using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataManager.Exportation;

internal class DataExportationProgressDto
{
    [JsonPropertyName(nameof(StartDate))]
    public string StartDate { get; set; }

    [JsonPropertyName(nameof(IsProcessing))]
    public bool IsProcessing { get; set; }

    [JsonPropertyName(nameof(Message))]
    public string Message { get; set; }

    [JsonPropertyName(nameof(FinishedMessage))]
    public string FinishedMessage { get; set; }

    [JsonPropertyName(nameof(HasError))]
    public bool HasError { get; set; }

    [JsonPropertyName(nameof(PercentProcess))]
    public int PercentProcess { get; set; }
}
