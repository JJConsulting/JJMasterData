using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using System.Data;
using System.Globalization;
using System.Reflection;
using JJMasterData.WebApi.Models;

namespace JJMasterData.WebApi.Services;

public class AccountService
{
    private string? ApiVersion { get; }
    private DataAccess DataAccess { get; }

    public AccountService()
    {
        ApiVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        DataAccess = new DataAccess();
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
            cmd.Parameters.Add(new DataAccessParameter("@password", Cript.EnigmaEncryptRP(password)));
            cmd.Parameters.Add(new DataAccessParameter("@appID", appId));

            var col = DataAccess.GetFields(cmd);
            if (col != null)
            {
                ret.ErrorId = int.Parse(col["ErrorId"]?.ToString() ?? string.Empty);
                ret.Message = Translate.Key(col["Message"]?.ToString());
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
            Log.AddError(ex, ex.Message);
            ret.IsValid = false;
            ret.Message = ExceptionManager.GetMessage(ex);
            ret.ErrorId = 100;
        }

        return ret;
    }


    public UserAccessInfo ChangePassword(AccountChange form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(AccountChange));

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
            cmd.Parameters.Add(new DataAccessParameter("@pwdCurrent", Cript.EnigmaEncryptRP(pwdCurrent)));
            cmd.Parameters.Add(new DataAccessParameter("@pwdNew", Cript.EnigmaEncryptRP(pwdNew)));
            cmd.Parameters.Add(new DataAccessParameter("@pwdConfirm", Cript.EnigmaEncryptRP(pwdConfirm)));

            var col = DataAccess.GetFields(cmd);
            if (col != null)
            {
                ret.ErrorId = int.Parse(col["ErrorId"]?.ToString() ?? string.Empty);
                ret.Message = Translate.Key(col["Message"]?.ToString());
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
            Log.AddError(ex.Message);

            ret.IsValid = false;
            ret.Message = ExceptionManager.GetMessage(ex);
            ret.ErrorId = 100;
        }

        return ret;
    }

    public UserAccessInfo RecoverPassword(AccountRecover form)
    {
        if (form == null)
            throw new ArgumentNullException(nameof(AccountChange));

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
                ret.ErrorId = int.Parse(col["ErrorId"]?.ToString() ?? string.Empty);
                ret.Message = Translate.Key(col["Message"]?.ToString());
                ret.IsValid = "1".Equals(col["IsValid"]?.ToString());

                if (ret.IsValid)
                {
                    ret.UserId = col["UserId"]?.ToString();
                    ret.Token = BuildToken(ret.UserId);
                    ret.Version = ApiVersion;

                    if (ret.ErrorId == 101)
                    {
                        string? email = col["Email"]?.ToString();
                        string pwd = Cript.EnigmaDecryptRP(col["Password"]?.ToString());
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
            Log.AddError(ex.Message);

            ret.IsValid = false;
            ret.Message = ExceptionManager.GetMessage(ex);
            ret.ErrorId = 100;
        }

        return ret;
    }


    private void NotifyPasswordRecovered(int userId, string email, string password)
    {
        ConfigSmtp config = GetConfigSmtp(userId);
        string subject = Translate.Key("Recover password") + " " + Translate.Key("WebSales");
        string body = Translate.Key("Your current password is:") + " " + password;

        Email.SendMailConfig(email, null, "", subject, body, false, null, config);
    }

    public string BuildToken(string? userId)
    {
        string token = $"{userId}|{ApiVersion}|{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        token = Cript.EnigmaEncryptRP(token);
        return Cript.Cript64(token);
    }

    public static TokenInfo? GetTokenInfo(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        TokenInfo? info = null;
        try
        {
            string infoToken = Cript.EnigmaDecryptRP(Cript.Descript64(token));
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

    private static string? GetParam(string param, object? userId)
    {
        string? value = null;
        var dao = new DataAccess();
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

        object result = dao.GetResult(cmd);
        if (result != null)
            value = result.ToString();

        return value;
    }

    private static ConfigSmtp GetConfigSmtp(int userId)
    {
        var config = new ConfigSmtp
        {
            Server = GetParam("SmtpServer", userId),
            Port = int.Parse(GetParam("EmailPortNumber", userId) ?? string.Empty),
            Email = GetParam("FromEmail", userId),
            User = GetParam("EmailUser", userId),
            Password = Cript.EnigmaDecryptRP(GetParam("EmailPassword", userId)),
            EnableSSL = GetParam("EmailSSL", userId)!.Equals("True")
        };

        return config;
    }

}