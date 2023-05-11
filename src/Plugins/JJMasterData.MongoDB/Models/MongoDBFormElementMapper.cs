using JJMasterData.Core.DataDictionary;
using MongoDB.Bson;

namespace JJMasterData.MongoDB.Models;

internal static class MongoDBFormElementMapper
{
    public static MongoDBFormElement FromFormElement(FormElement formElement)
    {
        return new MongoDBFormElement
        {
            //todo;
        };
    }
    
    public static FormElement FromMongoDBMetadata(MongoDBFormElement mongoDBFormElement)
    {
        return mongoDBFormElement;
    }
}