#if NETFRAMEWORK || NETSTANDARD
using System.Collections.Generic;

namespace JJMasterData.Commons.Extensions;

public static class DictionaryExtensions
{
    public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
    {
        return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
    }
}
#endif  