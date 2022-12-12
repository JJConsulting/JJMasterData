using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class MetadataInfo
{
    [DataMember(Name = "name")]
    public string Name { get; set; }
    
    [DataMember(Name = "namefilter")]
    public string NameFilter { get; set; }
    
    [DataMember(Name = "type")]
    public string Type { get; set; }
    
    [DataMember(Name = "tablename")]
    public string TableName { get; set; }
    
    [DataMember(Name = "info")]
    public string Info { get; set; }
    
    [DataMember(Name = "owner")]
    public string Owner { get; set; }
    
    [DataMember(Name = "sync")]
    public string Sync { get; set; }
    
    [DataMember(Name = "modified")]
    public DateTime LastModified { get; set; }
    
    [DataMember(Name = "json")]
    public string Json { get; set; }

    public MetadataInfo()
    {
        
    }

    public MetadataInfo(Metadata metadata, DateTime lastModified)
    {
        Type = "F";
        Name = metadata.Table.Name;
        NameFilter = metadata.Table.Name;
        TableName = metadata.Table.TableName;
        Info = metadata.Table.Info;
        Owner = "";
        Sync = metadata.Table.Sync ? "1" : "0";
        LastModified = lastModified;
    }
    
}