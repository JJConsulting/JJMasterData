using System.Collections;

namespace JJMasterData.MongoDB.Models;

public record MongoDBOrderByMapper(string FieldName, string Type)
{
    public IDictionary ToDictionary()
    {
        return new Dictionary<string, int>
        {
            { FieldName, Type == "ASC" ? 1 : -1 }
        };
    }

    public void Deconstruct(out string fieldName, out string type)
    {
        fieldName = FieldName;
        type = Type;
    }
}