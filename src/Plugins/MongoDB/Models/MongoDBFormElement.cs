using JJMasterData.Core.DataDictionary;
using MongoDB.Bson.Serialization.Attributes;

namespace JJMasterData.MongoDB.Models;

[BsonIgnoreExtraElements]
[BsonNoId]
internal class MongoDBFormElement : FormElement
{
    public DateTime LastModified { get; set; }
}