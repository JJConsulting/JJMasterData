using System.Collections;
using System.Collections.Generic;

namespace JJMasterData.Commons.Extensions;

public static class HashtableInteropExtensions
{
    public static Hashtable ToHashtable(this IDictionary<string, object> dictionary)
    {
        var hashtable = new Hashtable();
        foreach (var kvp in dictionary)
        {
            hashtable[kvp.Key] = kvp.Value;
        }
        return hashtable;
    }

    public static IDictionary<string, object> ToDictionary(this Hashtable hashtable)
    {
        var dictionary = new Dictionary<string, object>();
        foreach (DictionaryEntry entry in hashtable)
        {
            dictionary[entry.Key.ToString()] = entry.Value;
        }
        return dictionary;
    }
}