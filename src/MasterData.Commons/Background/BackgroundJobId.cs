using System;
using System.Security.Cryptography;
using System.Text;

namespace JJMasterData.Commons.Background;

public static class BackgroundJobId
{
    public static Guid Create(string operation, string elementName, string userId)
    {
        var value = string.Join('\0', operation, elementName, userId);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return new Guid(hash.AsSpan(0, 16));
    }
}
