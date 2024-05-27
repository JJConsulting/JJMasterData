#nullable enable
namespace JJMasterData.Commons.Security.Cryptography.Abstractions;

public interface IEncryptionService
{
    string EncryptString(string plainText, string? secretKey = null);
    string DecryptString(string cipherText, string? secretKey = null);
}