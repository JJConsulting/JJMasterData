using JJMasterData.Commons.Security.Cryptography.Abstractions;

namespace JJMasterData.Protheus.Security;

internal class ProtheusEncryptionService : IEncryptionService
{
    /// <summary>
    /// Encrypts a Protheus password.
    /// </summary>
    /// <param name="password">Password</param>
    /// <remarks>Kleberton 2012-08-28</remarks>
    public string EncryptString(string password)
    {
        string protheusPassword = "";

        int[] odd = { 1, 3, 5, 7, 9, 11 };
        int[] even = { 0, 2, 4, 6, 8, 10 };
        
        for (int i = 0; i < password.Length; i++)
        {
            if (odd[i] < password.Length)
            {
                protheusPassword += password.ToCharArray()[odd[i]];
            }
            else
            {
                break;
            }
        }
        
        for (int i = 0; i < password.Length; i++)
        {
            if (even[i] < password.Length)
            {
                protheusPassword += password.ToCharArray()[even[i]];
            }
            else
            {
                break;
            }
        }
        return protheusPassword;
    }

    /// <summary>
    /// Descript a Protheus password from SRA
    /// </summary>
    /// <param name="password">Password</param>
    /// <remarks>Lucio Pelinson 2012-09-21</remarks>
    public string DecryptString(string password)
    {
        string protheusPassword = "";
        var evenList = new List<char>();
        var oddList = new List<char>();
        char[] passwordArray = password.ToCharArray();
        int passwordLengthByHalf = password.Length / 2;

        for (int i = 0; i < password.Length; i++)
        {
            if (i < passwordLengthByHalf)
                evenList.Add(passwordArray[i]);
            else
                oddList.Add(passwordArray[i]);
        }


        int charCount = evenList.Count >= oddList.Count ? evenList.Count : oddList.Count;
        for (int i = 0; i < charCount; i++)
        {
            if (i < oddList.Count)
            {
                protheusPassword += oddList[i];
            }

            if (i < evenList.Count)
            {
                protheusPassword += evenList[i];
            }
        }

        return protheusPassword;
    }
}