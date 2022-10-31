using System;
using System.IO;
using System.Text;
using System.Web;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

public class JJDownloadFile : JJBaseView
{
    public const string DirectDownloadParameter = "jjdirectdownload";
    public const string DownloadParameter = "jjdownload";
    
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

    internal override HtmlBuilder RenderHtml()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new Exception(Translate.Key("Invalid file path or badly formatted URL"));
    
        if (IsExternalLink)
            return GetDownloadHtmlElement();
        
        ResponseDirectDownload();
    
        return null;
    }

    private HtmlBuilder GetDownloadHtmlElement()
    {
        var file = new FileInfo(FilePath);
        string fileName = file.Name;
        string size = Format.FormatFileSize(file.Length);
        string lastWriteTime = file.LastWriteTime.ToDateTimeString();
        string url = CurrentContext.Request.AbsoluteUri.Replace(DirectDownloadParameter, DownloadParameter);

        var html = new HtmlBuilder(HtmlTag.Div)
            .AppendElement(new JJTitle(Translate.Key("Downloading"),fileName.ToLower()))
            .AppendElement(HtmlTag.Section, section =>
            {
                section.WithCssClass("container mt-3");
                section.AppendElement(HtmlTag.Div, div =>
                {
                    div.WithCssClass("jumbotron px-3 py-4 px-sm-4 py-sm-5 bg-light rounded-3 mb-3");
                    div.AppendElement(HtmlTag.Div, div =>
                    {
                        div.AppendElement(HtmlTag.H1, h1 =>
                        {
                            h1.AppendElement(new JJIcon("fa fa-cloud-download text-info"));
                            h1.AppendText(fileName);
                        });
                        div.AppendElement(HtmlTag.P, p =>
                        {
                            p.AppendText($"{Translate.Key("File Size:")} {size}");
                            p.AppendElement(HtmlTag.Br);
                            p.AppendText($"{Translate.Key("Last write time:")} {lastWriteTime}");
                        });
                        div.AppendElement(HtmlTag.Hr, hr =>
                        {
                            hr.WithCssClass("my-4");
                        });
                        div.AppendElement(HtmlTag.P, p =>
                        {
                            p.AppendText(Translate.Key("You are downloading file {0}.", fileName));
                            p.AppendText(" ");
                            p.AppendText(Translate.Key("If the download not start automatically") + ", ");
                            p.AppendElement(HtmlTag.A, a =>
                            {
                                a.WithAttribute("href", url);
                                a.AppendText(Translate.Key("click here."));
                            });
                        });
                    });
                });
            });
        return html;
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
        if (view.CurrentContext.Request.QueryString(DirectDownloadParameter) != null)
            return true;
        if (view.CurrentContext.Request.QueryString(DownloadParameter) != null)
            return true;
        return false;
    }

    public static HtmlBuilder ResponseRoute(JJBaseView view)
    {
        bool isExternalLink = false;
        string criptFilePath = view.CurrentContext.Request.QueryString(DownloadParameter);
        if (criptFilePath == null)
        {
            criptFilePath = view.CurrentContext.Request.QueryString(DirectDownloadParameter);
            isExternalLink = true;
        }

        if (criptFilePath == null)
            return null;

        string filePath = Cript.Descript64(criptFilePath);
        if (filePath == null)
            throw new Exception(Translate.Key("Invalid file path or badly formatted URL"));

        var download = new JJDownloadFile
        {
            FilePath = filePath,
            IsExternalLink = isExternalLink
        };

        return download.GetHtmlBuilder();
    }


    
}
