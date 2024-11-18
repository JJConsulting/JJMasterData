using System.Collections;
using System.Text.Json.Serialization;


namespace JJMasterData.WebApi.Models;


public class DicSyncParam
{
    /// <summary>
    /// Nome do dicionário
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Filtros a serem aplicados no count.
    /// Array com o nome do campo e valor
    /// </summary>
    [JsonPropertyName("filters")]
    public Hashtable? Filters { get; set; }
}