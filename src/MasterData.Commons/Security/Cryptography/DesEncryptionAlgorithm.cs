using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JJMasterData.Commons.Security.Cryptography.Abstractions;

namespace JJMasterData.Commons.Security.Cryptography;

/// <summary>
/// DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.
/// </summary>
public class DesEncryptionAlgorithm : IEncryptionAlgorithm
{
    private static readonly byte[] Iv = [12, 34, 56, 78, 90, 102, 114, 126];
    
    public string EncryptString(string plainText, string secretKey)
    {
        using var des = DES.Create();
        byte[] input = Encoding.UTF8.GetBytes(plainText); 
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey[..8]);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, des.CreateEncryptor(keyBytes, Iv), CryptoStreamMode.Write))
        {
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();    
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public string DecryptString(string cipherText, string secretKey)
    {
        if (cipherText == null)
            return null;
        try
        {
            using var des = DES.Create();
            using var ms = new MemoryStream();
            var input = Convert.FromBase64String(cipherText.Replace(" ", "+"));
            var keyBytes = Encoding.UTF8.GetBytes(secretKey.Substring(0, 8));
            using (var cs = new CryptoStream(ms, des.CreateDecryptor(keyBytes, Iv), CryptoStreamMode.Write))
            {
                cs.Write(input, 0, input.Length);
                cs.FlushFinalBlock();    
            }
            
            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch
        {
            return null;
        }
        
    }
}