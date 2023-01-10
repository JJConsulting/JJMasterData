namespace JJMasterData.Commons.Cryptography.Abstractions;

public interface IEncryptionService
{
    public string EncryptString(string plainText, string key);
    public string DecryptString(string cipherText, string key);
}