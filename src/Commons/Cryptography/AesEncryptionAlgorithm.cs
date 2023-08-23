using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JJMasterData.Commons.Cryptography.Abstractions;

namespace JJMasterData.Commons.Cryptography;

/// <summary>
/// AES is more secure than the DES cipher and is the de facto world standard. DES can be broken easily as it has known vulnerabilities.
/// </summary>
public class AesEncryptionAlgorithm : IEncryptionAlgorithm
{
    public string EncryptString(string plainText, string secretKey)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);

        using var sha256 = SHA256.Create();
        byte[] aesKey = sha256.ComputeHash(keyBytes);

        using var md5 = MD5.Create();
        byte[] aesIv = md5.ComputeHash(keyBytes);
        
        using var aes = Aes.Create();
        aes.Key = aesKey;
        aes.IV = aesIv;

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
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] buffer = Convert.FromBase64String(cipherText);

            using var sha256 = SHA256.Create();
            byte[] aesKey = sha256.ComputeHash(keyBytes);

            using var md5 = MD5.Create();
            byte[] aesIv = md5.ComputeHash(keyBytes);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIv;
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
}