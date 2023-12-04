using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    [Display(Name = "Element Name")]
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
    [Display(Name = "Table Name")]
    public string TableName { get; set; }

    [JsonProperty("useReadProcedure")] 
    [Display(Name = "Use Read Procedure")]
    public bool UseReadProcedure { get; set; }
    
    [JsonProperty("customprocnameget")]
    [Display(Name = "Read Procedure")]
    public string ReadProcedureName { get; set; }
    
    [JsonProperty("useWriteProcedure")] 
    [Display(Name = "Use Write Procedure")]
    public bool UseWriteProcedure { get; set; }
    
    [JsonProperty("customprocnameset")]
    [Display(Name = "Write Procedure")]
    public string WriteProcedureName { get; set; }
    
    [JsonProperty("sync")]
    public bool EnableSynchronism { get; set; }

    /// <summary>
    /// Works online or offline on synchronized devices
    /// </summary>
    /// <remarks>
    /// Behavior whether data will be synchronized online or offline
    /// <para>Default = SyncMode.Online</para>
    /// </remarks>
    [JsonProperty("mode")]
    public SynchronismMode SynchronismMode { get; set; }

    public Element()
    {
        Fields = new ElementFieldList();
        Indexes = new List<ElementIndex>();
        Relationships = new List<ElementRelationship>();
        SynchronismMode = SynchronismMode.Online;
        EnableSynchronism = false;
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