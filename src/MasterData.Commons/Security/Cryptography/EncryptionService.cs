#nullable enable

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JetBrains.Annotations;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Security.Cryptography;

[Obsolete("Please use Microsoft IDataProtectionProvider")]
public class EncryptionService(IOptionsSnapshot<MasterDataCommonsOptions> options) : IEncryptionService
{
    private readonly string _secretKey = options.Value.SecretKey!;

    public string EncryptString(string plainText)
    {
        return AesEncryptionAlgorithm.EncryptString(plainText, _secretKey);
    }

    public string DecryptString(string cipherText)
    {
        return AesEncryptionAlgorithm.DecryptString(cipherText, _secretKey);
    }
}

internal sealed class AesEncryptionAlgorithm
{
    public static string EncryptString(string plainText, string secretKey)
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
    
    public static string DecryptString(string cipherText, string secretKey)
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
    private static Aes CreateAes(string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var aesKey = SHA256.HashData(keyBytes);
        var aesIv = MD5.HashData(keyBytes);

        return CreateAes(aesKey, aesIv);
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