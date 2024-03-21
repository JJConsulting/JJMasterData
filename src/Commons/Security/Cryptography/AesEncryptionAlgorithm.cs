using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace JJMasterData.Commons.Security.Cryptography;

/// <summary>
/// AES is more secure than the DES cipher and is the de facto world standard. DES can be broken easily as it has known vulnerabilities.
/// </summary>
public class AesEncryptionAlgorithm(IMemoryCache memoryCache) : IEncryptionAlgorithm
{
    private record AesEntry(byte[] Key, byte[] IV);
    
    public string EncryptString(string plainText, string secretKey)
    {
        using var aes = CreateAes(secretKey);

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using (var streamWriter = new StreamWriter(cryptoStream))
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
            var decrypt = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decrypt, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch
        {
            return null;
        }
    }
    
    private Aes CreateAes(string secretKey)
    {
        Aes aes = null;
        try
        {
            if (memoryCache.TryGetValue(secretKey, out AesEntry aesEntry))
            {
                aes = CreateAes(aesEntry);
                return aes;
            }
            
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);

            using var sha256 = SHA256.Create();
            var aesKey = sha256.ComputeHash(keyBytes);

            using var md5 = MD5.Create();
            var aesIv = md5.ComputeHash(keyBytes);

            aesEntry = new AesEntry(aesKey, aesIv);
            
            memoryCache.Set(secretKey, aesEntry);
            
            aes = CreateAes(aesEntry);

            return aes;
        }
        catch
        {
            aes?.Dispose();
            throw;
        }
    }

    private static Aes CreateAes(AesEntry aesEntry)
    {
        var aes = Aes.Create();
        aes.Key = aesEntry.Key;
        aes.IV = aesEntry.IV;
        return aes;
    }
}