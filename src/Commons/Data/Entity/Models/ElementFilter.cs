﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Informações de filtro
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementFilter(FilterMode type)
{
    /// <summary>
    /// Filter type
    /// </summary>
    [JsonPropertyName("type")]
    [Display(Name = "Filter Mode")]
    public FilterMode Type { get; set; } = type;

    /// <summary>
    /// Required filter
    /// </summary>
    [JsonPropertyName("isrequired")]
    [Display(Name = "Required")]
    public bool IsRequired { get; set; }

    public ElementFilter() : this(FilterMode.None)
    {
    }

    public ElementFilter DeepCopy() => (ElementFilter)MemberwiseClone();
}