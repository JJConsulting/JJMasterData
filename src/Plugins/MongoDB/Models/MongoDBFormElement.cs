using JJMasterData.Core.DataDictionary.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace JJMasterData.MongoDB.Models;

[BsonIgnoreExtraElements]
[BsonNoId]
internal sealed class MongoDBFormElement(FormElement formElement)
{
    public FormElement FormElement { get; set; } = formElement;
    public DateTime LastModified { get; set; } = DateTime.Now;
}