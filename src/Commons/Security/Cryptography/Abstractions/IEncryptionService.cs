namespace JJMasterData.Commons.Security.Cryptography.Abstractions;

public interface IEncryptionService
{
    string EncryptString(string plainText);
    string DecryptString(string cipherText);
}