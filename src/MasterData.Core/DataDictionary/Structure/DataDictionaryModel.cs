using System;
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Structure;

public class DataDictionaryModel
{
    public string Name { get; set; }
    public char Type { get; set; }
    public DateTime Modified { get; set; }
    public string Json { get; set; }

    public static DataDictionaryModel FromDictionary(Dictionary<string, object> dictionary)
    {
        return new DataDictionaryModel
        {
            Name = dictionary.TryGetValue("name", out var value) ? value.ToString() : null,
            Type = dictionary.TryGetValue("type", out var value1) ? Convert.ToChar(value1) : default,
            Modified = dictionary.TryGetValue("modified", out var value2) ? Convert.ToDateTime(value2) : default,
            Json = dictionary.TryGetValue("json", out var value3) ? value3.ToString() : null
        };
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dictionary = new Dictionary<string, object>
        {
            { "Name", Name },
            { "Type", Type },
            { "Modified", Modified },
            { "Json", Json }
        };
        return dictionary;
    }
}