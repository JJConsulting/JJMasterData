using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary>
/// Represents a filter element with a specified filter mode and requirement status.
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class ElementFilter(FilterMode type)
{
    /// <summary>
    /// Gets or sets the filter mode.
    /// </summary>
    [JsonPropertyName("type")]
    [Display(Name = "Filter Mode")]
    public FilterMode Type { get; set; } = type;

    /// <summary>
    /// Gets or sets a value indicating whether the filter is required.
    /// </summary>
    [JsonPropertyName("isrequired")]
    [Display(Name = "Required")]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ElementFilter"/> class with the default filter mode.
    /// </summary>
    public ElementFilter() : this(FilterMode.None)
    {
    }
    
    public ElementFilter DeepCopy() => (ElementFilter)MemberwiseClone();
}