namespace JJMasterData.Commons.Cryptography.Abstractions;

/// <summary>
/// Represents a secure encryption service.
/// </summary>
public interface IEncryptionService
{
    public string EncryptString(string plainText, string key);
    public string DecryptString(string cipherText, string key);
}