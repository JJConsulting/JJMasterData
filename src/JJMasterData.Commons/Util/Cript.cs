using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.DI;

namespace JJMasterData.Commons.Util;

[Obsolete("This class violates the SRP principle. Please use each equivalent service.")]
public class Cript
{
    private static byte[] _chave = { };
    private static readonly byte[] Iv = { 12, 34, 56, 78, 90, 102, 114, 126 };

    /// <summary>
    /// Encrypts a text.
    /// </summary>
    /// <param name="value">Value to be encrypted.</param>
    /// <returns>Encrypted value.</returns>
    [Obsolete("DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.")]
    public static string Cript64(string value)
    {
        return Cript64(value, JJService.CommonsOptions.SecretKey);
    }

    /// <summary>
    /// Encrypts a text.
    /// </summary>
    /// <param name="value">Value to be encrypted.</param>
    /// <param name="secretKey">Secret key.</param>
    /// <returns>Encrypted value.</returns>
    [Obsolete("DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.")]
    public static string Cript64(string value, string secretKey)
    {
        var des = new DESCryptoServiceProvider();
        var ms = new MemoryStream();
        var input = Encoding.UTF8.GetBytes(value); _chave = Encoding.UTF8.GetBytes(secretKey.Substring(0, 8));
        var cs = new CryptoStream(ms, des.CreateEncryptor(_chave, Iv), CryptoStreamMode.Write);
        cs.Write(input, 0, input.Length);
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Descripts a text
    /// </summary>
    /// <param name="value">Value to be descrypted.</param>
    /// <returns>Descrypted value.</returns>
    [Obsolete("DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.")]
    public static string Descript64(string value)
    {
        return Descript64(value, JJService.CommonsOptions.SecretKey);
    }

    /// <summary>
    /// Descripts a text
    /// </summary>
    /// <param name="value">Value to be descrypted.</param>
    /// <param name="secretKey">Secret key.</param>
    /// <returns>Descrypted value.</returns>
    [Obsolete("DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.")]
    public static string Descript64(string value, string secretKey)
    {
        if (value == null)
            return null;

        try
        {
            var des = new DESCryptoServiceProvider();
            using var ms = new MemoryStream();
            
            var input = Convert.FromBase64String(value.Replace(" ", "+"));

            _chave = Encoding.UTF8.GetBytes(secretKey.Substring(0, 8));

            using var cs = new CryptoStream(ms, des.CreateDecryptor(_chave, Iv), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch
        {
            return null;
        }
    }
    
    [Obsolete("Please use ProtheusEncryptionService.")]
    public static string CriptPwdProtheus(string password)
    {
        return ProtheusEncryptionService.EncryptPassword(password);
    }

    [Obsolete("Please use ProtheusEncryptionService.")]
    public static string DeCriptPwdProtheus(string password)
    {
        return ProtheusEncryptionService.DecryptPassword(password);
    }
    
    [Obsolete("Please use MD5HashHelper.")]
    public static string GetMd5Hash(string input)
    {
        return Md5HashHelper.GetMd5Hash(input);
    }

    [Obsolete("Please use MD5HashHelper.")]
    public static bool VerifyMd5Hash(string input, string hash)
    {
        return Md5HashHelper.VerifyMd5Hash(input, hash);
    }

    [Obsolete("Please use ReportPortalEnigmaService.")]
    public static string EnigmaEncryptRP(string message, string secretKey = "Secret")
    {
        string encryptRp = "";
        if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(secretKey))
        {
            return message;
        }

        int tam = secretKey.Length;
        int pos = 1;
        while (pos <= message.Length)
        {
            char @char = message.Substring(pos - 1, 1).ToCharArray()[0];
            int num = @char;

            @char = secretKey.Substring(pos % tam, 1).ToCharArray()[0];
            int num3 = @char;

            int number = num + num3;
            if (number > 255)
            {
                number = 255;
            }

            encryptRp += ("0" + number.ToString("X")).Substring(("0" + number.ToString("X")).Length - 2);
            pos++;
        }

        return encryptRp;
    }

    [Obsolete("Please use ReportPortalEnigmaService.")]
    public static string EnigmaDecryptRP(string message, string secretKey = "Secret")
    {
        return new ReportPortalEnigmaService().EncryptString(message, secretKey);
    }

}
