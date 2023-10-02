#if NET48
using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Configuration.Options;
using JJMasterData.Commons.Security.Cryptography;
using JJMasterData.Commons.Security.Hashing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Commons.Util;

/// <summary>
/// Static acessor to encryption services.
/// </summary>
[Obsolete("This class is controlled by a static service locator.")]
public class Cript
{
    private const string SecretKeyErrorMessage = "You must config or pass a secret key";
    private static DesEncryptionAlgorithm _desEncryptionAlgorithm;
    private static ReportPortalEnigmaAlgorithm _reportPortalEnigmaAlgorithm;

    private static DesEncryptionAlgorithm DesEncryptionAlgorithm => _desEncryptionAlgorithm ??= new DesEncryptionAlgorithm();

    private static ReportPortalEnigmaAlgorithm ReportPortalEnigmaAlgorithm =>
        _reportPortalEnigmaAlgorithm ??= new ReportPortalEnigmaAlgorithm();

    /// <summary>
    /// Encrypts a text.
    /// DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.
    /// </summary>
    public static string Cript64(string value)
    {
        return Cript64(value, StaticServiceLocator.Provider.GetRequiredService<IOptions<JJMasterDataCommonsOptions>>().Value.SecretKey);
    }

    public static string Cript64(string value, string secretKey)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey), SecretKeyErrorMessage);

        return DesEncryptionAlgorithm.EncryptString(value, secretKey);
    }

    /// <summary>
    /// Decrypts a text.
    /// DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.
    /// </summary>
    public static string Descript64(string value)
    {
        return Descript64(value, StaticServiceLocator.Provider.GetRequiredService<IOptions<JJMasterDataCommonsOptions>>().Value.SecretKey);
    }

    public static string Descript64(string value, string secretKey)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey), SecretKeyErrorMessage);
        
        return DesEncryptionAlgorithm.DecryptString(value, secretKey);
    }

    public static string GetMd5Hash(string input)
    {
        return Md5HashHelper.ComputeHash(input);
    }

    public static bool VerifyMd5Hash(string input, string hash)
    {
        return Md5HashHelper.VerifyHash(input, hash);
    }

    public static string EnigmaEncryptRP(string message, string secretKey = "Secret")
    {
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey), SecretKeyErrorMessage);
        
        return ReportPortalEnigmaAlgorithm.EncryptString(message, secretKey);
    }

    public static string EnigmaDecryptRP(string message, string secretKey = "Secret")
    {
        if (string.IsNullOrEmpty(secretKey))
            throw new ArgumentNullException(nameof(secretKey), SecretKeyErrorMessage);
        
        return ReportPortalEnigmaAlgorithm.DecryptString(message, secretKey);
    }
}
#endif
