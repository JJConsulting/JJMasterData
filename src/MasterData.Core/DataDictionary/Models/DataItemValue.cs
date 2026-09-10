#nullable disable warnings
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Object responsible for storing the items of a list
/// </summary>

public class DataItemValue
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [CanBeNull]
    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
    
    [JsonPropertyName("icon")]
    public FontAwesomeIcon? Icon { get; set; }

    /// <summary>
    /// Image color in hexadecimal.
    /// </summary>
    /// <example>
    /// #FF112F1
    /// </example>
    [JsonPropertyName("imagecolor")]
    public string? IconColor { get; set; }

    [JsonPropertyName("group")]
    public string? Group { get; set; }

    public DataItemValue() { }
    
    [SetsRequiredMembers]
    public DataItemValue(string id, string description)
    {
        Id = id;
        Description = description;
    }
    
    [SetsRequiredMembers]
    public DataItemValue(string id, string description, FontAwesomeIcon icon, string iconColor)
    {
        Id = id;
        Description = description;
        Icon = icon;
        IconColor = iconColor;
    }

    public DataItemValue DeepCopy()
    {
        return (DataItemValue)MemberwiseClone();
    }
}
