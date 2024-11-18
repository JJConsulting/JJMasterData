#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;


namespace JJMasterData.Commons.Data.Entity.Models;

/// <summary> 
/// Metadata representation of a database element.
/// </summary>
/// <remarks>
/// 2017-03-22 - JJTeam
/// </remarks>
[DebuggerDisplay("Name = {Name}")]
public class Element()
{
    [JsonPropertyName("schema")]
    [Display(Name = "Schema")]
    public string? Schema { get; set; } 
    
    [JsonPropertyName("name")]
    [Display(Name = "Element Name")]
    public required string Name { get; set; }
    
    [JsonPropertyName("info")]
    public string? Info { get; set; }
    
    [JsonPropertyName("fields")]
    public ElementFieldList Fields { get; set; } = [];

    [JsonPropertyName("indexes")]
    public List<ElementIndex> Indexes { get; set; } = [];

    [JsonPropertyName("relations")]
    public List<ElementRelationship> Relationships { get; set; } = [];

    [JsonPropertyName("tableName")]
    [Display(Name = "Table Name")]
    public string TableName { get; set; } = null!;

    [JsonPropertyName("useReadProcedure")] 
    [Display(Name = "Use Read Procedure")]
    public bool UseReadProcedure { get; set; }
    
    [JsonPropertyName("customprocnameget")]
    [Display(Name = "Read Procedure")]
    public string? ReadProcedureName { get; set; } 
    
    [JsonPropertyName("useWriteProcedure")] 
    [Display(Name = "Use Write Procedure")]
    public bool UseWriteProcedure { get; set; }
    
    [JsonPropertyName("customprocnameset")]
    [Display(Name = "Write Procedure")]
    public string? WriteProcedureName { get; set; }
    
    [JsonPropertyName("sync")]
    public bool EnableSynchronism { get; set; } = false;

    /// <summary>
    /// Works online or offline on synchronized devices
    /// </summary>
    /// <remarks>
    /// Behavior whether data will be synchronized online or offline
    /// <para>Default = SyncMode.Online</para>
    /// </remarks>
    [JsonPropertyName("mode")]
    public SynchronismMode SynchronismMode { get; set; } = SynchronismMode.Online;

    /// <summary>
    /// Custom connection string. If null, will use the default JJMasterData:ConnectionString from IConfiguration.
    /// </summary>
    [Display(Name = "Connection String")]
    [JsonPropertyName("connectionId")]
    public Guid? ConnectionId { get; set; }
    
    [SetsRequiredMembers]
    public Element(string name) : this()
    {
        Name = name;
        TableName = name;
    }

    [SetsRequiredMembers]
    public Element(string name, string description) : this(name)
    {
        Info = description;
    }
    
    public List<ElementField> GetPrimaryKeys()
    {
        return Fields.FindAll(x => x.IsPk);
    }
}