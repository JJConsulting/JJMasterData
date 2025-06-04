#nullable disable
using Newtonsoft.Json;

namespace JJMasterData.LegacyMetadataMigrator.FormElementMigration;

public class MetadataInfo
{
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("tablename")]
    public string TableName { get; set; }
    
    [JsonProperty("info")]
    public string Info { get; set; }
        
    [JsonProperty("sync")]
    public string Sync { get; set; }
    
    [JsonProperty("modified")]
    public DateTime Modified { get; set; }
    
    public MetadataInfo()
    {
        
    }

    public MetadataInfo(Metadata metadata, DateTime modified)
    {
        Name = metadata.Table.Name;
        TableName = metadata.Table.TableName;
        Info = metadata.Table.Info;
        Sync = metadata.Table.EnableSynchronism ? "1" : "0";
        Modified = modified;
    }
    
}