using System.Collections;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JJMasterData.WebApi.Models;


public class DicSyncParam
{
    /// <summary>
    /// Nome do dicionário
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Filtros a serem aplicados no count.
    /// Array com o nome do campo e valor
    /// </summary>
    [JsonProperty("filters")]
    public Hashtable? Filters { get; set; }
}