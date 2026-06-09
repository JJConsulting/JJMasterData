#nullable enable

using System;
using System.IO;
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

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using (var streamWriter = new StreamWriter(cryptoStream, Encoding.UTF8))
        {
            streamWriter.Write(plainText);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }
    
    public string DecryptString(string cipherText, string secretKey)
    {
        try
        {
            using var aes = CreateAes(secretKey);
            var buffer = Convert.FromBase64String(cipherText);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream, Encoding.UTF8);

            return streamReader.ReadToEnd();
        }
        catch
        {
            return string.Empty;
        }
    }
    
    [MustDisposeResource]
    private static Aes CreateAes(string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var aesKey = SHA256.HashData(keyBytes);
        var aesIv = MD5.HashData(keyBytes);

        var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIv;
        return aes;
    }
}