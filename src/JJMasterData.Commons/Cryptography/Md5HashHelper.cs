using System;
using System.Security.Cryptography;
using System.Text;

namespace JJMasterData.Commons.Cryptography;

public static class Md5HashHelper
{
    public static string GetMd5Hash(string input)
    {
        byte[] data;
        using (var md5Hasher = MD5.Create())
        {
            data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        }
        
        var stringBuilder = new StringBuilder();

        for (int i = 0; i <= data.Length - 1; i++)
        {
            stringBuilder.Append(data[i].ToString("x2"));
        }

        return stringBuilder.ToString();
    }
    
    public static bool VerifyMd5Hash(string input, string hash)
    {
        string hashOfInput = GetMd5Hash(input);
        var comparer = StringComparer.OrdinalIgnoreCase;

        return 0 == comparer.Compare(hashOfInput, hash);
    }
}