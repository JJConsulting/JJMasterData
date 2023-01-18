using System;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Data.Entity;

/// <summary>
/// Informações de filtro
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class ElementFilter
{
    /// <summary>
    /// Filter type
    /// </summary>
    [DataMember(Name = "type")]
    public FilterMode Type { get; set; }

    /// <summary>
    /// Required filter
    /// </summary>
    [DataMember(Name = "isrequired")]
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