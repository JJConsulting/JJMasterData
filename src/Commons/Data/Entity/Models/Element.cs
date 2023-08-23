using System.Collections.Generic;
using Newtonsoft.Json;

namespace JJMasterData.Commons.Data.Entity;

/// <summary> 
/// Elemento base com a estrutura basica da tabela
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
public class Element
{
    /// <summary>
    /// Dictionary Name
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Table information
    /// </summary>
    [JsonProperty("info")]
    public string Info { get; set; }

    /// <summary>
    /// Field List
    /// </summary>
    [JsonProperty("fields")]
    public ElementFieldList Fields { get; set; }

    /// <summary>
    /// Index List
    /// </summary>
    [JsonProperty("indexes")]
    public List<ElementIndex> Indexes { get; set; }

    /// <summary>
    /// Relationships List
    /// </summary>
    [JsonProperty("relations")]
    public List<ElementRelationship> Relationships { get; set; }

    /// <summary>
    /// Table Name
    /// </summary>
    [JsonProperty("tableName")]
    public string TableName { get; set; }

    /// <summary>
    /// Custom name for recording procedure
    /// </summary>
    [JsonProperty("customprocnameget")]
    public string CustomProcNameGet { get; set; }

    /// <summary>
    /// Custom name for read procedure
    /// </summary>
    [JsonProperty("customprocnameset")]
    public string CustomProcNameSet { get; set; }

    /// <summary>
    /// Send dictionary between applications
    /// </summary>
    /// <remarks>
    /// Enable sync for the app.
    /// <para></para>Default = true
    /// </remarks>
    [JsonProperty("sync")]
    public bool EnableWebApi { get; set; }

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
        EnableWebApi = false;
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