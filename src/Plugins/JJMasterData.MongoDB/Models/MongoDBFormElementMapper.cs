using JJMasterData.Core.DataDictionary;
using MongoDB.Bson;

namespace JJMasterData.MongoDB.Models;

internal static class MongoDBFormElementMapper
{
    public static MongoDBFormElement FromFormElement(FormElement formElement)
    {
        return new MongoDBFormElement
        {
            LastModified = DateTime.Now
        };
    }
    

}