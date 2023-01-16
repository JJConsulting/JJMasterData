using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.DI;

namespace JJMasterData.Commons.Util;

/// <summary>
/// Static acessor to encryption services.
/// </summary>
public class Cript
{
    private static DesEncryptionService _desEncryptionService;
    private static ReportPortalEnigmaService _reportPortalEnigmaService;

    private static DesEncryptionService DesEncryptionService => _desEncryptionService ??= new DesEncryptionService();

    private static ReportPortalEnigmaService ReportPortalEnigmaService =>
        _reportPortalEnigmaService ??= new ReportPortalEnigmaService();

    /// <summary>
    /// Encrypts a text.
    /// DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.
    /// </summary>
    public static string Cript64(string value)
    {
        return Cript64(value, JJService.CommonsOptions.SecretKey);
    }


    public static string Cript64(string value, string secretKey)
    {
        return DesEncryptionService.EncryptString(value, secretKey);
    }

    /// <summary>
    /// Decrypts a text.
    /// DES algorithm can be broken easily as it has known vulnerabilities. Please use AesEncryptionService.
    /// </summary>
    public static string Descript64(string value)
    {
        return Descript64(value, JJService.CommonsOptions.SecretKey);
    }

    public static string Descript64(string value, string secretKey)
    {
        return DesEncryptionService.DecryptString(value, secretKey);
    }

    public static string CriptPwdProtheus(string password)
    {
        return ProtheusEncryptionService.EncryptPassword(password);
    }

    public static string DeCriptPwdProtheus(string password)
    {
        return ProtheusEncryptionService.DecryptPassword(password);
    }

    public static string GetMd5Hash(string input)
    {
        return Md5HashHelper.GetMd5Hash(input);
    }

    public static bool VerifyMd5Hash(string input, string hash)
    {
        return Md5HashHelper.VerifyMd5Hash(input, hash);
    }

    public static string EnigmaEncryptRP(string message, string secretKey = "Secret")
    {
        return ReportPortalEnigmaService.EncryptString(message, secretKey);
    }

    public static string EnigmaDecryptRP(string message, string secretKey = "Secret")
    {
        return ReportPortalEnigmaService.DecryptString(message, secretKey);
    }
}