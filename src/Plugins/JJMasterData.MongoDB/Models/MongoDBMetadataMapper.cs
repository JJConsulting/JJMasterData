using JJMasterData.Core.DataDictionary;
using MongoDB.Bson;

namespace JJMasterData.MongoDB.Models;

internal static class MongoDBMetadataMapper
{
    public static MongoDBMetadata FromMetadata(Metadata metadata)
    {
        return new MongoDBMetadata
        {
            ApiOptions = metadata.ApiOptions,
            Form = metadata.Form,
            Table = metadata.Table,
            Options = metadata.Options
        };
    }
    
    public static Metadata FromMongoDBMetadata(MongoDBMetadata mongoDbMetadata)
    {
        return new Metadata
        {
            ApiOptions = mongoDbMetadata.ApiOptions,
            Form = mongoDbMetadata.Form,
            Table = mongoDbMetadata.Table,
            Options = mongoDbMetadata.Options
        };
    }
}