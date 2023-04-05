using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Data.Entity;

/// <summary> 
/// Elemento base com a estrutura basica da tabela
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class Element 
{
    /// <summary>
    /// Dictionary Name
    /// </summary>
    [DataMember(Name = "name")]
    public string Name { get; set; }

    /// <summary>
    /// Table information
    /// </summary>
    [DataMember(Name = "info")]
    public string Info { get; set; }
    /// <summary>
    /// Field List
    /// </summary>
    [DataMember(Name = "fields")]
    public ElementList Fields { get; set; }

    /// <summary>
    /// Index List
    /// </summary>
    [DataMember(Name = "indexes")]
    public List<ElementIndex> Indexes { get; set; }

    /// <summary>
    /// Relationships List
    /// </summary>
    [DataMember(Name = "relations")]
    public List<ElementRelationship> Relationships { get; set; }

    /// <summary>
    /// Table Name
    /// </summary>
    [DataMember(Name = "tableName")]
    public string TableName { get; set; } 

    /// <summary>
    /// Custom name for recording procedure
    /// </summary>
    [DataMember(Name = "customprocnameget")]
    public string CustomProcNameGet { get; set; }

    /// <summary>
    /// Custom name for read procedure
    /// </summary>
    [DataMember(Name = "customprocnameset")]
    public string CustomProcNameSet { get; set; }

    /// <summary>
    /// Send dictionary between applications
    /// </summary>
    /// <remarks>
    /// Enable sync for the app.
    /// <para></para>Default = true
    /// </remarks>
    [DataMember(Name = "sync")]
    public bool Sync { get; set; }

    /// <summary>
    /// Works online or offline on synchronized devices
    /// </summary>
    /// <remarks>
    /// Behavior whether data will be synchronized online or offline
    /// <para>Default = SyncMode.Online</para>
    /// </remarks>
    [DataMember(Name = "mode")]
    public SyncMode SyncMode { get; set; }

    public Element()
    {
        Fields = new ElementList();
        Indexes = new List<ElementIndex>();
        Relationships = new List<ElementRelationship>();
        SyncMode = SyncMode.Online;
        Sync = false;
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