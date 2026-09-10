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
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));

        return Convert.ToHexString(bytes);
    }
}
