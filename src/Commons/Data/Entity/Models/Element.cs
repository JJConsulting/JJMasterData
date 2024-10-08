﻿#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

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
    [JsonProperty("schema")]
    [Display(Name = "Schema")]
    public string? Schema { get; set; } 
    
    [JsonProperty("name")]
    [Display(Name = "Element Name")]
    public required string Name { get; set; }
    
    [JsonProperty("info")]
    public string? Info { get; set; }
    
    [JsonProperty("fields")]
    public ElementFieldList Fields { get; set; } = [];

    [JsonProperty("indexes")]
    public List<ElementIndex> Indexes { get; set; } = [];

    [JsonProperty("relations")]
    public List<ElementRelationship> Relationships { get; set; } = [];

    [JsonProperty("tableName")]
    [Display(Name = "Table Name")]
    public required string TableName { get; set; }

    [JsonProperty("useReadProcedure")] 
    [Display(Name = "Use Read Procedure")]
    public bool UseReadProcedure { get; set; }
    
    [JsonProperty("customprocnameget")]
    [Display(Name = "Read Procedure")]
    public string? ReadProcedureName { get; set; } 
    
    [JsonProperty("useWriteProcedure")] 
    [Display(Name = "Use Write Procedure")]
    public bool UseWriteProcedure { get; set; }
    
    [JsonProperty("customprocnameset")]
    [Display(Name = "Write Procedure")]
    public string? WriteProcedureName { get; set; }
    
    [JsonProperty("sync")]
    public bool EnableSynchronism { get; set; } = false;

    /// <summary>
    /// Works online or offline on synchronized devices
    /// </summary>
    /// <remarks>
    /// Behavior whether data will be synchronized online or offline
    /// <para>Default = SyncMode.Online</para>
    /// </remarks>
    [JsonProperty("mode")]
    public SynchronismMode SynchronismMode { get; set; } = SynchronismMode.Online;

    /// <summary>
    /// Custom connection string. If null, will use the default JJMasterData:ConnectionString from IConfiguration.
    /// </summary>
    [Display(Name = "Connection String")]
    [JsonProperty("connectionId")]
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