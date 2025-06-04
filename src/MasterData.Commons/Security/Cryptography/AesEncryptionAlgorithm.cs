#nullable enable

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using JJMasterData.Commons.Security.Cryptography.Abstractions;

namespace JJMasterData.Commons.Security.Cryptography;

/// <summary>
/// AES is more secure than the DES cipher and is the de facto world standard. DES can be broken easily as it has known vulnerabilities.
/// </summary>
public class AesEncryptionAlgorithm : IEncryptionAlgorithm
{
    private readonly ConcurrentDictionary<string, (byte[] Key, byte[] IV)> _aesCache = new();

    public string EncryptString(string plainText, string secretKey)
    {
        using var aes = CreateAes(secretKey);

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

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
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using var memoryStream = new MemoryStream(buffer);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
        catch
        {
            return string.Empty;
        }
    }
    
    [MustDisposeResource]
    private Aes CreateAes(string secretKey)
    {
        if (_aesCache.TryGetValue(secretKey, out var aesEntry))
        {
            return CreateAes(aesEntry.Key, aesEntry.IV);
        }
        
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);

        using var sha256 = SHA256.Create();
        var aesKey = sha256.ComputeHash(keyBytes);

        using var md5 = MD5.Create();
        var aesIv = md5.ComputeHash(keyBytes);

        aesEntry = new(aesKey, aesIv);
        
        _aesCache.TryAdd(secretKey, aesEntry);

        return CreateAes(aesEntry.Key, aesEntry.IV);
    }

    [MustDisposeResource]
    private static Aes CreateAes(byte[] key, byte[] iv)
    {
        var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        return aes;
    }
}