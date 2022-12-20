using System.Collections;

namespace JJMasterData.MongoDB.Models;

public record MongoDBOrderByMapper(string FieldName, string Type)
{
    public IDictionary ToDictionary()
    {
        return new Dictionary<string, int>()
        {
            { FieldName, Type == "ASC" ? 1 : -1 }
        };
    }
}