using JJMasterData.Commons.Data;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Options;
using JJMasterData.WebApi.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Data;
using System.Globalization;
using System.Reflection;
using JJMasterData.Commons.Security.Cryptography;
using JJMasterData.Commons.Security.Cryptography.Abstractions;

namespace JJMasterData.WebApi.Services;

public class AccountService
{
    private IEncryptionService EncryptionService { get; }
    private ReportPortalEnigmaAlgorithm ReportPortalEnigmaAlgorithm { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataCoreOptions Options { get; }
    private ILogger<AccountService> Logger { get; }
    private string? ApiVersion { get; }
    private DataAccess DataAccess { get; }

    public AccountService(
        IConfiguration configuration,
        IEncryptionService encryptionService,
        ReportPortalEnigmaAlgorithm reportPortalEnigmaAlgorithm,
        IOptions<JJMasterDataCoreOptions> options,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILogger<AccountService> logger)
    {
        EncryptionService = encryptionService;
        ReportPortalEnigmaAlgorithm = reportPortalEnigmaAlgorithm;
        StringLocalizer = stringLocalizer;
        Options = options.Value;
        Logger = logger;
        ApiVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        DataAccess = new DataAccess(configuration);
    }
    public UserAccessInfo Login(string? username, string? password, string? appId)
    {
        var ret = new UserAccessInfo();
        try
        {
            var cmd = new DataAccessCommand
            {
                Sql = "jj_dologin",
                CmdType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add(new DataAccessParameter("@username", username));
            cmd.Parameters.Add(new DataAccessParameter("@password", ReportPortalEnigmaAlgorithm.DecryptString(password, Options.SecretKey)));
            cmd.Parameters.Add(new DataAccessParameter("@appID", appId));

            var col = DataAccess.GetFields(cmd);
            if (col != null)
            {
                ret.ErrorId = int.Parse(col["ErrorId"]?.ToString() ?? string.Empty);
                if (col.ContainsKey("Message"))
                    ret.Message = StringLocalizer[col["Message"]?.ToString()!];

                ret.IsValid = "1".Equals(col["IsValid"]?.ToString());

                if (ret.IsValid)
                {
                    ret.UserId = col["UserId"]?.ToString();
                    ret.Token = BuildToken(ret.UserId);
                    ret.Version = ApiVersion;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Login generate a generic error");
            ret.IsValid = false;
            ret.Message = ExceptionManager.GetMessage(ex);
            ret.ErrorId = 100;
        }

        return ret;
    }


    public UserAccessInfo ChangePassword(AccountChange form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        return ChangePassword(form.User, form.PwdCurrent, form.PwdNew, form.PwdConfirm);
    }


    public UserAccessInfo ChangePassword(string? username, string? pwdCurrent, string? pwdNew, string? pwdConfirm)
    {
        var ret = new UserAccessInfo();
        try
        {
            var cmd = new DataAccessCommand
            {
                Sql = "jj_changepassword",
                CmdType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new DataAccessParameter("@username", username));
            cmd.Parameters.Add(new DataAccessParameter("@pwdCurrent", ReportPortalEnigmaAlgorithm.EncryptString(pwdCurrent, Options.SecretKey)));
            cmd.Parameters.Add(new DataAccessParameter("@pwdNew", ReportPortalEnigmaAlgorithm.EncryptString(pwdNew, Options.SecretKey)));
            cmd.Parameters.Add(new DataAccessParameter("@pwdConfirm", ReportPortalEnigmaAlgorithm.EncryptString(pwdConfirm, Options.SecretKey)));

            var col = DataAccess.GetFields(cmd);
            if (col != null)
            {
                if (col.ContainsKey("Message"))
                    ret.Message = StringLocalizer[col["Message"]?.ToString()!];

                ret.ErrorId = int.Parse(col["ErrorId"]?.ToString() ?? string.Empty);
                ret.IsValid = "1".Equals(col["IsValid"]?.ToString());

                if (ret.IsValid)
                {
                    ret.UserId = col["UserId"]?.ToString();
                    ret.Token = BuildToken(ret.UserId);
                    ret.Version = ApiVersion;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "ChangePassword generate a generic error");
            ret.IsValid = false;
            ret.Message = ExceptionManager.GetMessage(ex);
            ret.ErrorId = 100;
        }

        return ret;
    }

    public UserAccessInfo RecoverPassword(AccountRecover form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(form));

        return RecoverPassword(form.User, form.AppId);
    }


    public UserAccessInfo RecoverPassword(string? username, string? appId)
    {
        var ret = new UserAccessInfo();
        try
        {
            var cmd = new DataAccessCommand
            {
                Sql = "jj_recoverpassword",
                CmdType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new DataAccessParameter("@username", username));
            cmd.Parameters.Add(new DataAccessParameter("@appID", appId));

            var col = DataAccess.GetFields(cmd);
            if (col != null)
            {
                if (col.ContainsKey("Message"))
                    ret.Message = StringLocalizer[col["Message"]?.ToString()!];

                ret.ErrorId = int.Parse(col["ErrorId"]?.ToString() ?? string.Empty);
                ret.IsValid = "1".Equals(col["IsValid"]?.ToString());

                if (ret.IsValid)
                {
                    ret.UserId = col["UserId"]?.ToString();
                    ret.Token = BuildToken(ret.UserId);
                    ret.Version = ApiVersion;

                    if (ret.ErrorId == 101)
                    {
                        string? email = col["Email"]?.ToString();
                        string pwd = ReportPortalEnigmaAlgorithm.DecryptString(col["Password"]?.ToString(), Options.SecretKey);
                        if (ret.UserId != null)
                        {
                            int userId = int.Parse(ret.UserId);

                            if (email != null)
                                NotifyPasswordRecovered(userId, email, pwd);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "RecoverPassword generate a generic error");
            ret.IsValid = false;
            ret.Message = ExceptionManager.GetMessage(ex);
            ret.ErrorId = 100;
        }

        return ret;
    }


    private void NotifyPasswordRecovered(int userId, string email, string password)
    {
        ConfigSmtp config = GetConfigSmtp(userId);
        string subject = $"{StringLocalizer["Recover password"]} {StringLocalizer["WebSales"]}";
        string body = $"{StringLocalizer["Your current password is:"]} {password}";

        Email.SendMailConfig(email, null, "", subject, body, false, null, config);
    }

    public string BuildToken(string? userId)
    {
        string token = $"{userId}|{ApiVersion}|{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        return EncryptionService.EncryptStringWithUrlEscape(token);
    }

    public TokenInfo? GetTokenInfo(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        TokenInfo? info = null;
        try
        {
            string infoToken = EncryptionService.DecryptStringWithUrlUnescape(token);
            if (infoToken != null)
            {
                string[] parms = infoToken.Split('|');
                if (parms.Length > 1)
                {
                    info = new TokenInfo
                    {
                        UserId = parms[0],
                        Version = parms[1],
                        CreatedDade = DateTime.ParseExact(parms[2], "yyyyMMddHHmmss", CultureInfo.InvariantCulture)
                    };
                }
            }
        }
        catch
        {
            info = null;
        }

        return info;
    }

    private string? GetParam(string param, object? userId)
    {
        string? value = null;
        var cmd = new DataAccessCommand
        {
            CmdType = CommandType.StoredProcedure,
            Sql = "jj_getparam"
        };
        cmd.Parameters.Add(new DataAccessParameter("@param", param));

        if (userId == null)
            cmd.Parameters.Add(new DataAccessParameter("@userid", DBNull.Value, DbType.Int32));
        else
            cmd.Parameters.Add(new DataAccessParameter("@userid", userId, DbType.Int32));

        object? result = DataAccess.GetResult(cmd);
        if (result != null)
            value = result.ToString();

        return value;
    }

    private ConfigSmtp GetConfigSmtp(int userId)
    {
        var config = new ConfigSmtp
        {
            Server = GetParam("SmtpServer", userId),
            Port = int.Parse(GetParam("EmailPortNumber", userId) ?? string.Empty),
            Email = GetParam("FromEmail", userId),
            User = GetParam("EmailUser", userId),
            Password = ReportPortalEnigmaAlgorithm.DecryptString(GetParam("EmailPassword", userId), Options.SecretKey),
            EnableSSL = GetParam("EmailSSL", userId)!.Equals("True")
        };

        return config;
    }

}