using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using JJMasterData.Commons.DI;

namespace JJMasterData.Commons.Util;

public class Cript
{
    private static byte[] _chave = { };
    private static readonly byte[] Iv = { 12, 34, 56, 78, 90, 102, 114, 126 };

    /// <summary>
    /// Encrypts a text.
    /// </summary>
    /// <param name="valor">Value to be encrypted.</param>
    /// <returns>Encrypted value.</returns>
    public static string Cript64(string valor)
    {
        return Cript64(valor, JJService.CommonsOptions.SecretKey);
    }

    /// <summary>
    /// Encrypts a text.
    /// </summary>
    /// <param name="valor">Value to be encrypted.</param>
    /// <param name="secretKey">Secret key.</param>
    /// <returns>Encrypted value.</returns>
    public static string Cript64(string valor, string secretKey)
    {
        var des = new DESCryptoServiceProvider();
        var ms = new MemoryStream();
        var input = Encoding.UTF8.GetBytes(valor); _chave = Encoding.UTF8.GetBytes(secretKey.Substring(0, 8));
        var cs = new CryptoStream(ms, des.CreateEncryptor(_chave, Iv), CryptoStreamMode.Write);
        cs.Write(input, 0, input.Length);
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Descripts a text
    /// </summary>
    /// <param name="valor">Value to be descrypted.</param>
    /// <returns>Descrypted value.</returns>
    public static string Descript64(string valor)
    {
        return Descript64(valor, JJService.CommonsOptions.SecretKey);
    }

    /// <summary>
    /// Descripts a text
    /// </summary>
    /// <param name="valor">Value to be descrypted.</param>
    /// <param name="secretKey">Secret key.</param>
    /// <returns>Descrypted value.</returns>
    public static string Descript64(string valor, string secretKey)
    {
        if (valor == null)
            return null;

        try
        {
            var des = new DESCryptoServiceProvider();
            var ms = new MemoryStream();
            
            var input = Convert.FromBase64String(valor.Replace(" ", "+"));

            _chave = Encoding.UTF8.GetBytes(secretKey.Substring(0, 8));

            var cs = new CryptoStream(ms, des.CreateDecryptor(_chave, Iv), CryptoStreamMode.Write);
            cs.Write(input, 0, input.Length);
            cs.FlushFinalBlock();

            return Encoding.UTF8.GetString(ms.ToArray());
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Encrypts a Protheus password.
    /// </summary>
    /// <param name="password">Password</param>
    /// <remarks>Kleberton 2012-08-28</remarks>
    public static string CriptPwdProtheus(string password)
    {
        string senhaCript = "";

        int[] impar = { 1, 3, 5, 7, 9, 11 };
        int[] par = { 0, 2, 4, 6, 8, 10 };

        //Caracters pares
        for (int i = 0; i < password.Length; i++)
        {
            if (impar[i] < password.Length)
            {
                senhaCript += password.ToCharArray()[impar[i]];
            }
            else
            {
                break;
            }
        }

        // Caracters impares
        for (int i = 0; i < password.Length; i++)
        {
            if (par[i] < password.Length)
            {
                senhaCript += password.ToCharArray()[par[i]];
            }
            else
            {
                break;
            }
        }
        return senhaCript;
    }

    /// <summary>
    /// Descript a Protheus password from SRA
    /// </summary>
    /// <param name="password">Password</param>
    /// <remarks>Lucio Pelinson 2012-09-21</remarks>
    public static string DeCriptPwdProtheus(string password)
    {
        string senhaDeCript = "";
        var aPar = new List<char>();
        var aImpar = new List<char>();
        char[] aSenha = password.ToCharArray();
        int nQtd = password.Length / 2;

        for (int i = 0; i < password.Length; i++)
        {
            if (i < nQtd)
                aPar.Add(aSenha[i]);
            else
                aImpar.Add(aSenha[i]);
        }


        int nQtdChar = aPar.Count >= aImpar.Count ? aPar.Count : aImpar.Count;
        for (int i = 0; i < nQtdChar; i++)
        {
            if (i < aImpar.Count)
            {
                senhaDeCript += aImpar[i];
            }

            if (i < aPar.Count)
            {
                senhaDeCript += aPar[i];
            }
        }

        return senhaDeCript;
    }

    /// <summary>
    /// Converts a string to MD5 hash (32 character hexadecimal string)
    /// </summary>
    /// <param name="input">Value to be converted</param>
    /// <returns>Value in hash</returns>
    /// <remarks>Lucio Pelinson 06-12-2013</remarks>
    public static string GetMd5Hash(string input)
    {
        var md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        var stringBuilder = new StringBuilder();

        for (int i = 0; i <= data.Length - 1; i++)
        {
            stringBuilder.Append(data[i].ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Verify MD5 hash
    /// </summary>
    /// <remarks>Lucio Pelinson 06-12-2013</remarks>
    public static bool VerifyMd5Hash(string input, string hash)
    {
        string hashOfInput = GetMd5Hash(input);
        var comparer = StringComparer.OrdinalIgnoreCase;

        return 0 == comparer.Compare(hashOfInput, hash);
    }

    /// <summary>
    /// Cript a ReportPortal message
    /// </summary>
    /// <param name="message">Text to be criptografed.</param>
    /// <remarks>
    /// See dbo.EnigmaEncrypt function in SQLServer
    /// </remarks>
    public static string EnigmaEncryptRP(string message)
    {
        return EnigmaEncryptRP(message, "Secret");
    }

    /// <summary>
    /// Descripts a message from ReportPortal
    /// </summary>
    /// <param name="message">Text to be encrypted.</param>
    /// <param name="secretKey">Secret key.</param>
    /// <remarks>
    /// See dbo.EnigmaEncrypt function in SQLServer
    /// </remarks>
    public static string EnigmaEncryptRP(string message, string secretKey)
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
            char c = message.Substring(pos - 1, 1).ToCharArray()[0];
            int num = c;

            c = secretKey.Substring(pos % tam, 1).ToCharArray()[0];
            int num3 = c;

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

    /// <summary>
    /// Descripts a message from ReportPortal
    /// </summary>
    /// <param name="message">Text to be encrypted.</param>
    /// <remarks>
    /// Veja a função dbo.EnigmaEncrypt no SQLServer
    /// </remarks>
    public static string EnigmaDecryptRP(string message)
    {
        return EnigmaDecryptRP(message, "Secret");
    }

    /// <summary>
    /// Descripts a message from ReportPortal
    /// </summary>
    /// <param name="message">Text to be descrypted.</param>
    /// <param name="secretKey">Chave secreta</param>
    public static string EnigmaDecryptRP(string message, string secretKey)
    {
        if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(secretKey))
            return null;

        string decryptRp = null;
        int tam = message.Length / 2;
        int pos = 1;

        while (pos <= tam)
        {
            int numM = Convert.ToInt32("0x" + message.Substring((pos - 1) * 2, 2), 16);
            short numP = (short)Convert.ToChar(secretKey.Substring(pos % secretKey.Length, 1));
            int charCode = (short)(numM - numP);

            decryptRp += (char)charCode;
            pos++;
        }

        return decryptRp;
    }

}
