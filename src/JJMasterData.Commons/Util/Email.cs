using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using JJMasterData.Commons.Logging;

namespace JJMasterData.Commons.Util;

public class Email
{

    public static bool SendMail(string toEmail, string ccEmail, string ccoEmail, string subject, string body, bool IsBodyHtml)
    {
        return SendMail(toEmail, ccEmail, ccoEmail, subject, body, IsBodyHtml, "");
    }

    public static bool SendMail(string toEmail, string ccEmail, string ccoEmail, string subject, string body, bool IsBodyHtml, string filePath)
    {
        List<Attachment> listAttach = null;
        if (filePath != null && filePath.Trim().Length > 0)
        {
            listAttach = new List<Attachment>();
            listAttach.Add(new Attachment(filePath));
        }

        return SendMail(toEmail, ccEmail, ccoEmail, subject, body, IsBodyHtml, listAttach);
    }

    public static bool SendMailConfig(string toEmail, string ccEmail, string ccoEmail, string subject, string body, bool IsBodyHtml, string filePath, string smtpconfig)
    {
        List<Attachment> listAttach = null;
        if (filePath != null && filePath.Trim().Length > 0)
        {
            listAttach = new List<Attachment>();
            listAttach.Add(new Attachment(filePath));
        }

        ConfigSmtp osmtpconfig = null;
        if (!string.IsNullOrEmpty(smtpconfig))
            osmtpconfig = new ConfigSmtp(smtpconfig);

        return SendMailConfig(toEmail, ccEmail, ccoEmail, subject, body, IsBodyHtml, listAttach, osmtpconfig);
    }

    public static bool SendMail(string toEmail, string ccEmail, string ccoEmail, string subject, string body, bool IsBodyHtml, string fileContent, string fileName)
    {
        List<Attachment> listAttach = null;
        if (fileName != null && fileName.Trim().Length > 0)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(fileContent);
            MemoryStream stream = new MemoryStream(byteArray);

            listAttach = new List<Attachment>();
            listAttach.Add(new Attachment(stream, fileName));
        }

        return SendMail(toEmail, ccEmail, ccoEmail, subject, body, IsBodyHtml, listAttach);
    }

    public static bool SendMail(string toEmail, string ccEmail, string ccoEmail, string subject, string body, bool IsBodyHtml, List<Attachment> listAttach)
    {
        return SendMailConfig(toEmail, ccEmail, ccoEmail, subject, body, IsBodyHtml, listAttach, null);
    }

    public static bool SendMailConfig(string toEmail, string ccEmail, string ccoEmail, string subject, string body, bool IsBodyHtml, List<Attachment> listAttach, ConfigSmtp smtpconfig)
    {
        bool lRet = true;
        SmtpClient oSmtp = null;
        MailMessage msgMail = null;

        try
        {
            msgMail = new MailMessage();
            if (!string.IsNullOrEmpty(toEmail))
            {
                msgMail.To.Add(toEmail.Replace(";", ",").Replace("\r\n", "").ToLower());
            }

            if (!string.IsNullOrEmpty(ccEmail))
            {
                msgMail.CC.Add(ccEmail.Replace(";", ",").ToLower());
            }

            if (!string.IsNullOrEmpty(ccoEmail))
            {
                msgMail.Bcc.Add(ccoEmail.Replace(";", ",").ToLower());
            }

            msgMail.Attachments.Clear();
            if (listAttach != null)
            {
                foreach (var atach in listAttach)
                    msgMail.Attachments.Add(atach);
            }

            msgMail.Subject = subject;
            msgMail.IsBodyHtml = IsBodyHtml;
            msgMail.Body = body;
            msgMail.SubjectEncoding = Encoding.GetEncoding("ISO-8859-1");

            //msgMail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            oSmtp = new SmtpClient();

            if (smtpconfig != null)
            {
                oSmtp.Host = smtpconfig.Server;
                oSmtp.Port = smtpconfig.Port;
                oSmtp.UseDefaultCredentials = false;

                NetworkCredential cred = new NetworkCredential(smtpconfig.User, smtpconfig.Password);
                oSmtp.Credentials = cred;
                oSmtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                oSmtp.EnableSsl = smtpconfig.EnableSSL;

                MailAddress mailfrom = new MailAddress(smtpconfig.Email);
                msgMail.From = mailfrom;
            }

            oSmtp.Send(msgMail);

            if (listAttach != null)
            {
                foreach (Attachment attachment in msgMail.Attachments)
                    attachment.Dispose();
            }
        }
        catch (Exception ex)
        {
            StringBuilder sErr = new StringBuilder();
            sErr.Append(ex.Message);
            if (ex.InnerException != null && ex.InnerException.Message != null)
            {
                sErr.AppendLine(" ");
                sErr.Append(ex.InnerException.Message);
            }
            Log.AddError(sErr.ToString(), "JJLIB");
            lRet = false;
        }
        finally
        {
            if (oSmtp != null)
            {
                oSmtp.Dispose();
            }

            if (msgMail != null)
            {
                msgMail.Dispose();
            }
        }

        return lRet;
    }


}

public class ConfigSmtp
{
    public string Email { get; set; }
    public string Server { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public bool EnableSSL { get; set; }

    public ConfigSmtp() { }

    public ConfigSmtp(string dataSource)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        string[] avalues = dataSource.Split(';');
        foreach (string item in avalues)
        {
            string[] info = item.Split('=');
            if (info.Length > 1)
                values.Add(info[0], info[1]);
        }

        if (values.Count > 0)
        {
            Email = values["EMAIL"];
            Server = values["SERVER"];
            Port = int.Parse(values["PORT"]);
            User = values["USER"];
            Password = values["PWD"];
            EnableSSL = values["SSL"] == "1";
        }
    }


    public override string ToString()
    {
        StringBuilder sVal = new StringBuilder();
        sVal.Append("EMAIL=");
        sVal.Append(Email);
        sVal.Append(";SERVER=");
        sVal.Append(Server);
        sVal.Append(";PORT=");
        sVal.Append(Port);
        sVal.Append(";USER=");
        sVal.Append(User);
        sVal.Append(";PWD=");
        sVal.Append(Password);
        sVal.Append(";SSL=");
        sVal.Append(EnableSSL ? "1" : "0");

        return sVal.ToString();
    }

}


