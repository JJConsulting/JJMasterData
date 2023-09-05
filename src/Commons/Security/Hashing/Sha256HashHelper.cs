using System;
using System.Security.Cryptography;
using System.Text;

namespace JJMasterData.Commons.Hashing;

public static class Sha256HashHelper
{
    public static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var inputBytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(inputBytes);
            
        var builder = new StringBuilder();
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }
    
    public static bool VerifyHash(string input, string hash)
    {
        var hashOfInput = ComputeHash(input);
        var comparer = StringComparer.OrdinalIgnoreCase;

        return 0 == comparer.Compare(hashOfInput, hash);
    }
}