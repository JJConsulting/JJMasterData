using System.Runtime.Serialization;

namespace JJMasterData.FormElementUpdater;

[Serializable]
[DataContract]
public class MetadataInfo
{
    [DataMember(Name = "name")]
    public string Name { get; set; }
    
    [DataMember(Name = "tablename")]
    public string TableName { get; set; }
    
    [DataMember(Name = "info")]
    public string Info { get; set; }
        
    [DataMember(Name = "sync")]
    public string Sync { get; set; }
    
    [DataMember(Name = "modified")]
    public DateTime Modified { get; set; }
    
    public MetadataInfo()
    {
        
    }

    public MetadataInfo(Metadata metadata, DateTime modified)
    {
        Name = metadata.Table.Name;
        TableName = metadata.Table.TableName;
        Info = metadata.Table.Info;
        Sync = metadata.Table.Sync ? "1" : "0";
        Modified = modified;
    }
    
}