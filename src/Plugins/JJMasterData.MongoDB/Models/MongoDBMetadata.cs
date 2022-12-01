using JJMasterData.Core.DataDictionary;
using MongoDB.Bson.Serialization.Attributes;

namespace JJMasterData.MongoDB.Models;

[BsonIgnoreExtraElements]
[BsonNoId]
internal class MongoDBMetadata : Metadata
{
    public DateTime LastModified { get; set; }
}