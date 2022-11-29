using JJMasterData.Core.DataDictionary;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JJMasterData.MongoDB.Models;

[BsonIgnoreExtraElements]
[BsonNoId]
public class MongoDBMetadata : Metadata { }