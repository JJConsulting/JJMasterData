using System;
using JJMasterData.Commons.Security.Cryptography.Abstractions;

namespace JJMasterData.Commons.Security.Cryptography;

public class ReportPortalEnigmaAlgorithm : IEncryptionAlgorithm
{
    public string EncryptString(string plainText, string secretKey)
    {
        string encryptRp = "";
        if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(secretKey))
        {
            return plainText;
        }

        int tam = secretKey.Length;
        int pos = 1;
        while (pos <= plainText.Length)
        {
            char @char = plainText.Substring(pos - 1, 1).ToCharArray()[0];
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

    public string DecryptString(string cipherText, string secretKey)
    {
        if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(secretKey))
            return null;

        string decryptRp = null;
        int tam = cipherText.Length / 2;
        int pos = 1;

        while (pos <= tam)
        {
            int numM = Convert.ToInt32($"0x{cipherText.Substring((pos - 1) * 2, 2)}", 16);
            short numP = (short)Convert.ToChar(secretKey.Substring(pos % secretKey.Length, 1));
            int charCode = (short)(numM - numP);

            decryptRp += (char)charCode;
            pos++;
        }

        return decryptRp;
    }
}