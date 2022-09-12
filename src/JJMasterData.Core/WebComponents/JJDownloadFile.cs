using System;
using System.IO;
using System.Text;
using System.Web;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;

namespace JJMasterData.Core.WebComponents;

public class JJDownloadFile : JJBaseView
{
    public const string PARAM_DIRECTDOWNLOAD = "jjdirectdownload";
    public const string PARAM_DOWNLOAD = "jjdownload";


    public JJDownloadFile()
    {

    }

    public JJDownloadFile(string filePath)
    {
        FilePath = filePath;
    }

    public string FilePath { get; set; }

    public bool IsExternalLink { get; set; }

    public static JJDownloadFile GetInstance()
    {
        return new JJDownloadFile();
    }
    
    protected override string RenderHtml()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new Exception(Translate.Key("Invalid file path or badly formatted URL"));

        if (IsExternalLink)
            return GetHtmlDownload();
        
        ResponseDirectDownload();

        return null;
    }

    private string GetHtmlDownload()
    {
        var file = new FileInfo(FilePath);
        string fileName = file.Name;
        string size = Format.FormatFileSize(file.Length);
        string lastWriteTime = file.LastWriteTime.ToDateTimeString();
        string url = CurrentContext.Request.AbsoluteUri.Replace(PARAM_DIRECTDOWNLOAD, PARAM_DOWNLOAD);

        var html = new StringBuilder();
        html.AppendLine("<div class=\"page-header \">");
        html.Append("	<h1>Downloading <small>");
        html.Append(fileName.ToLower());
        html.AppendLine("</small></h1>");
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"jumbotron\">");
        html.AppendLine("	<div class=\"container\">");
        html.AppendLine($"		<h1><span class=\"fa fa-cloud-download text-info\"></span>  {fileName}</h1>");
        html.Append("		<p>");
        html.Append(Translate.Key("File size:"));
        html.Append(' ');
        html.Append(size);
        html.Append("<br>");
        html.Append(Translate.Key("Last write time:"));
        html.Append(' ');
        html.Append(lastWriteTime);
        html.AppendLine("		</p>");
        html.AppendLine("	</div>");
        html.AppendLine("</div>");
        html.AppendLine("<div class=\"col-sm-12\">	");
        html.Append(Translate.Key("You are downloading file {0}.", fileName));
        html.Append(' ');
        html.Append(Translate.Key("If the download not start automatically"));
        html.Append($" <a href=\"{url}\">");
        html.Append(Translate.Key("click here."));
        html.Append("</a>");
        html.AppendLine("</div>");

        //Scripts
        html.AppendLine("<script type=\"text/javascript\"> ");
        html.AppendLine($"	window.location.assign('{url}');");
        html.AppendLine("</script> ");

        return html.ToString();
    }

    internal void ResponseDirectDownload()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new ArgumentNullException(nameof(FilePath));

        var fileName = FilePath.Substring(FilePath.LastIndexOf('\\') + 1);
        if (!File.Exists(FilePath))
        {
            Log.AddError(Translate.Key("File not found at {0}", FilePath));
            throw new Exception(Translate.Key("File {0} not found!", fileName));
        }

        CurrentContext.Response.ClearResponse();
        CurrentContext.Response.AddResponseHeader("Content-Type", MimeTypeUtil.GetMimeType(fileName));
        CurrentContext.Response.AddResponseHeader("Content-Transfer-Encoding", "binary");
        CurrentContext.Response.AddResponseHeader("Content-Description", "file Transfer");
        CurrentContext.Response.AddResponseHeader("Content-Disposition",
            $"attachment; filename={HttpUtility.UrlEncode(fileName)}");

        CurrentContext.Response.SendFile(FilePath);
    }

    public static bool IsDownloadRoute(JJBaseView view)
    {
        if (view.CurrentContext.Request.QueryString(PARAM_DIRECTDOWNLOAD) != null)
            return true;
        if (view.CurrentContext.Request.QueryString(PARAM_DOWNLOAD) != null)
            return true;
        return false;
    }

    public static string ResponseRoute(JJBaseView view)
    {
        bool isExternalLink = false;
        string criptFilePath = view.CurrentContext.Request.QueryString(PARAM_DOWNLOAD);
        if (criptFilePath == null)
        {
            criptFilePath = view.CurrentContext.Request.QueryString(PARAM_DIRECTDOWNLOAD);
            isExternalLink = true;
        }

        if (criptFilePath == null)
            return null;

        string filePath = Cript.Descript64(criptFilePath);
        if (filePath == null)
            throw new Exception(Translate.Key("Invalid file path or badly formatted URL"));

        var download = new JJDownloadFile();
        download.FilePath = filePath;
        download.IsExternalLink = isExternalLink;

        return download.GetHtml();
    }


    
}
