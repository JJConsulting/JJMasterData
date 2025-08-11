#nullable enable

using System;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using JJMasterData.Commons.Security.Cryptography.Abstractions;

namespace JJMasterData.Commons.Security.Cryptography;

/// <summary>
/// AES is more secure than the DES cipher and is the de facto world standard. DES can be broken easily as it has known vulnerabilities.
/// </summary>
public sealed class AesEncryptionAlgorithm : IEncryptionAlgorithm
{
    public string EncryptString(string plainText, string secretKey)
    {
        using var aes = CreateAes(secretKey);
        using var encryptor = aes.CreateEncryptor();
        
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        
        return Convert.ToBase64String(cipherBytes);
    }
    
    public string DecryptString(string cipherText, string secretKey)
    {
        using var aes = CreateAes(secretKey);
        using var decryptor = aes.CreateDecryptor();
        
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        
        return Encoding.UTF8.GetString(plainBytes);
    }
        
    [MustDisposeResource]
    private static Aes CreateAes(string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);

#if !NET
        using var sha256 = SHA256.Create();
        var aesKey = sha256.ComputeHash(keyBytes);
#else
        var aesKey = SHA256.HashData(keyBytes);
#endif

#if !NET
        using var md5 = MD5.Create();
        var aesIv = md5.ComputeHash(keyBytes);
#else
        var aesIv = MD5.HashData(keyBytes);
#endif
        var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIv;
        return aes;
    }
}