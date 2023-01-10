using JJMasterData.Core.DataDictionary;

namespace JJMasterData.MongoDB.Models;

internal static class MongoDBMetadataMapper
{
    public static MongoDBMetadata FromMetadata(Metadata metadata)
    {
        return new MongoDBMetadata
        {
            Api = metadata.Api,
            Form = metadata.Form,
            Table = metadata.Table,
            UIOptions = metadata.UIOptions
        };
    }
    
    public static Metadata FromMongoDBMetadata(MongoDBMetadata mongoDbMetadata)
    {
        return new Metadata
        {
            Api = mongoDbMetadata.Api,
            Form = mongoDbMetadata.Form,
            Table = mongoDbMetadata.Table,
            UIOptions = mongoDbMetadata.UIOptions
        };
    }
}