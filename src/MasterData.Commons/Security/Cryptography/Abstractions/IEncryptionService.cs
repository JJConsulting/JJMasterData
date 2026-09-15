#nullable enable
namespace JJMasterData.Commons.Security.Cryptography.Abstractions;

/// <summary>
/// Prefer using <see cref="DataProtectionService"/>.
/// </summary>
public interface IEncryptionService
{
    string EncryptString(string plainText);
    string DecryptString(string cipherText);
}