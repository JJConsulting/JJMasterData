namespace JJMasterData.Commons.Cryptography.Abstractions;

/// <summary>
/// Represents a secure encryption algorithm.
/// </summary>
public interface IEncryptionAlgorithm
{
    public string EncryptString(string plainText, string secretKey);
    public string DecryptString(string cipherText, string secretKey);
}