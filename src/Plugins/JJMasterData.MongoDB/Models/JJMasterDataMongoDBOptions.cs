namespace JJMasterData.MongoDB.Models;

public class JJMasterDataMongoDBOptions
{
    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;

    public string CollectionName { get; set; } = null!;
}