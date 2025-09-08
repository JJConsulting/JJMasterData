using System.Text.Json.Serialization;
using JetBrains.Annotations;


namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Object responsible for storing the items of a list
/// </summary>

public class DataItemValue
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("description")]
    [CanBeNull] 
    public string Description { get; set; }
    
    [CanBeNull]
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; }
    
    [JsonPropertyName("icon")]
    public IconType Icon { get; set; }

    /// <summary>
    /// Image color in hexadecimal.
    /// </summary>
    /// <example>
    /// #FF112F1
    /// </example>
    [JsonPropertyName("imagecolor")]
    public string IconColor { get; set; }

    [JsonPropertyName("group")] 
    [CanBeNull]
    public string Group { get; set; }

    public DataItemValue() { }
    
    public DataItemValue(string id, string description)
    {
        Id = id;
        Description = description;
    }
    public DataItemValue(string id, string description, IconType icon, string iconColor)
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