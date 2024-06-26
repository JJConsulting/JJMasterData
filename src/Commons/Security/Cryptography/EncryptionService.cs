#nullable enable
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Security.Cryptography;

/// <summary>
/// Wrapper to IEncryptionService with the secret key loaded by IOptions.
/// </summary>
public class EncryptionService(
    IEncryptionAlgorithm encryptionAlgorithm,
    IOptionsSnapshot<MasterDataCommonsOptions> options)
    : IEncryptionService
{
    private readonly string _secretKey = options.Value.SecretKey!;

    public string EncryptString(string plainText, string? secretKey = null)
    {
        return encryptionAlgorithm.EncryptString(plainText,secretKey ??_secretKey);
    }

    public string DecryptString(string cipherText, string? secretKey = null)
    {
        return encryptionAlgorithm.DecryptString(cipherText,secretKey ?? _secretKey);
    }
}