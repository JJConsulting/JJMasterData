using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JJMasterData.Commons.Util;

public static class DictionaryHash
{
    public static string ComputeHash(Dictionary<string, object> dict)
    {
        var ordered = dict.OrderBy(x => x.Key);
        var json = JsonSerializer.Serialize(ordered);
        
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(json));
        
        #if NETFRAMEWORK
        return BitConverter.ToString(bytes).Replace("-", "");
        #else
        return Convert.ToHexString(bytes);
        #endif
    }
}