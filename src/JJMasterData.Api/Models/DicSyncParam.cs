using System.Collections;
using System.Runtime.Serialization;

namespace JJMasterData.Api.Models;

[Serializable]
[DataContract]
public class DicSyncParam
{
    /// <summary>
    /// Nome do dicionário
    /// </summary>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary>
    /// Filtros a serem aplicados no count.
    /// Array com o nome do campo e valor
    /// </summary>
    [DataMember(Name = "filters")]
    public Hashtable Filters { get; set; }
}