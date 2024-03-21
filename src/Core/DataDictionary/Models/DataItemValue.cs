using JetBrains.Annotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

/// <summary>
/// Object responsible for storing the items of a list
/// </summary>

public class DataItemValue
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("description")]
    [CanBeNull] 
    public string Description { get; set; }
    
    [JsonProperty("icon")]
    public IconType Icon { get; set; }

    /// <summary>
    /// Image color in hexadecimal.
    /// </summary>
    /// <example>
    /// #FF112F1
    /// </example>
    [JsonProperty("imagecolor")]
    public string IconColor { get; set; }

    [JsonProperty("group")] 
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