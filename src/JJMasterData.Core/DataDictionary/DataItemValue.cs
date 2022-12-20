using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

/// <summary>
/// Object responsible for storing the items of a list
/// </summary>
[Serializable]
[DataContract]
public class DataItemValue
{
    
    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "description")]
    public string Description { get; set; }
    
    [DataMember(Name = "icon")]
    public IconType Icon { get; set; }

    /// <summary>
    /// Image color in hexadecimal.
    /// </summary>
    /// <example>
    /// #FF112F1
    /// </example>
    [DataMember(Name = "imagecolor")]
    public string ImageColor { get; set; }


    public DataItemValue() { }
    
    public DataItemValue(string id, string description)
    {
        Id = id;
        Description = description;
    }
    public DataItemValue(string id, string description, IconType icon, string imageColor)
    {
        Id = id;
        Description = description;
        Icon = icon;
        ImageColor = imageColor;
    }

}