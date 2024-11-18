

using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataManager.Importation;

internal record DataImportationDto
{
    [JsonPropertyName("StartDate")]
    public string StartDate { get; set; }

    [JsonPropertyName("Insert")]
    public int Insert { get; set; }

    [JsonPropertyName("Update")]
    public int Update { get; set; }

    [JsonPropertyName("Delete")]
    public int Delete { get; set; }

    [JsonPropertyName("Error")]
    public int Error { get; set; }

    [JsonPropertyName("Ignore")]
    public int Ignore { get; set; }

    [JsonPropertyName("IsProcessing")]
    public bool IsProcessing { get; set; }

    [JsonPropertyName("PercentProcess")]
    public int PercentProcess { get; set; }

    [JsonPropertyName("Message")]
    public string Message { get; set; }

   
}
