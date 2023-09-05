namespace JJMasterData.Commons.Cryptography;

public interface IEncryptionService
{
    string EncryptString(string plainText);
    string DecryptString(string cipherText);
}