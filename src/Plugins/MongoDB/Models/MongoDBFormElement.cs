using JJMasterData.Core.DataDictionary;
using MongoDB.Bson.Serialization.Attributes;

namespace JJMasterData.MongoDB.Models;

[BsonIgnoreExtraElements]
[BsonNoId]
internal class MongoDBFormElement 
{
    public FormElement FormElement { get; set; }
    public DateTime LastModified { get; set; }
    public MongoDBFormElement(FormElement formElement)
    {
        FormElement = formElement;
        LastModified = DateTime.Now;
    }
}