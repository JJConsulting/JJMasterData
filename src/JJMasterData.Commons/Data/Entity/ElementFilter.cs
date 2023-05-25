using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity;

/// <summary>
/// Informações de filtro
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementFilter
{
    /// <summary>
    /// Filter type
    /// </summary>
    [JsonProperty("type")]
    public FilterMode Type { get; set; }

    /// <summary>
    /// Required filter
    /// </summary>
    [JsonProperty("isrequired")]
    public bool IsRequired { get; set; }

    public ElementFilter()
    {
        Type = FilterMode.None;
    }

    public ElementFilter(FilterMode type)
    {
        Type = type;
    }
}