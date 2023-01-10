using JJMasterData.Commons.Cryptography.Abstractions;
using JJMasterData.Commons.Options;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Cryptography;

/// <summary>
/// Wrapper to IEncryptionService with the secret key loaded by IOptions.
/// </summary>
public class JJMasterDataEncryptionService
{
    private readonly string _secretKey;
    private readonly IEncryptionService _encryptionService;

    public JJMasterDataEncryptionService(
        IEncryptionService encryptionService,
        IOptions<JJMasterDataCommonsOptions> options)
    {
        _encryptionService = encryptionService;
        _secretKey = options.Value.SecretKey;
    }

    public string EncryptString(string plainText) => _encryptionService.EncryptString(plainText, _secretKey);

    public string DecryptString(string cipherText) =>
        _encryptionService.DecryptString(cipherText,_secretKey);
}