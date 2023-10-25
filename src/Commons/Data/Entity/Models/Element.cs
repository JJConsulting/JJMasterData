using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary> 
/// Represents a database element.
/// </summary>
/// <remarks>
/// Created at 2017-03-22 by JJTeam
/// </remarks>
public class Element
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("info")]
    public string Info { get; set; }
    
    [JsonProperty("fields")]
    public ElementFieldList Fields { get; set; }
    
    [JsonProperty("indexes")]
    public List<ElementIndex> Indexes { get; set; }
    
    [JsonProperty("relations")]
    public List<ElementRelationship> Relationships { get; set; }
    
    [JsonProperty("tableName")]
    public string TableName { get; set; }


    [JsonProperty("customprocnameget")]
    public string ReadProcedureName { get; set; }
    
    [JsonProperty("customprocnameset")]
    public string WriteProcedureName { get; set; }
    
    [JsonProperty("sync")]
    public bool EnableApi { get; set; }

    /// <summary>
    /// Works online or offline on synchronized devices
    /// </summary>
    /// <remarks>
    /// Behavior whether data will be synchronized online or offline
    /// <para>Default = SyncMode.Online</para>
    /// </remarks>
    [JsonProperty("mode")]
    public SyncMode SyncMode { get; set; }

    public Element()
    {
        Fields = new ElementFieldList();
        Indexes = new List<ElementIndex>();
        Relationships = new List<ElementRelationship>();
        SyncMode = SyncMode.Online;
        EnableApi = false;
    }

    public Element(string name) : this()
    {
        Name = name;
        TableName = name;
    }

    public Element(string name, string description) : this(name)
    {
        Info = description;
    }
}