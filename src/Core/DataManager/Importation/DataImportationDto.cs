using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Importation;

internal record DataImportationDto
{
    [JsonProperty("StartDate")]
    public string StartDate { get; set; }

    [JsonProperty("Insert")]
    public int Insert { get; set; }

    [JsonProperty("Update")]
    public int Update { get; set; }

    [JsonProperty("Delete")]
    public int Delete { get; set; }

    [JsonProperty("Error")]
    public int Error { get; set; }

    [JsonProperty("Ignore")]
    public int Ignore { get; set; }

    [JsonProperty("IsProcessing")]
    public bool IsProcessing { get; set; }

    [JsonProperty("PercentProcess")]
    public int PercentProcess { get; set; }

    [JsonProperty("Message")]
    public string Message { get; set; }

   
}
