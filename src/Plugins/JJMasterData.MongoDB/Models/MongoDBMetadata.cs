using JJMasterData.Core.DataDictionary;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JJMasterData.MongoDB.Models;

[BsonIgnoreExtraElements]
public class MongoDBMetadata : Metadata
{
    [BsonId]
    public ObjectId Id { get; set; }
}