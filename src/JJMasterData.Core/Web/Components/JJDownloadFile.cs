using System;
using System.Globalization;
using System.IO;
using System.Web;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http;

namespace JJMasterData.Core.Web.Components;

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

    internal override HtmlBuilder RenderHtml()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new JJMasterDataException(Translate.Key("Invalid file path or badly formatted URL"));

        if (IsExternalLink)
            return GetDownloadHtmlElement();

        DirectDownload();

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
            .AppendElement(new JJTitle(Translate.Key("Downloading"), fileName.ToLower()))
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
                        div.AppendElement(HtmlTag.Hr, hr => { hr.WithCssClass("my-4"); });
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

    internal void DirectDownload()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new ArgumentNullException(nameof(FilePath));

        if (!File.Exists(FilePath))
        {
            var exception = new JJMasterDataException(Translate.Key("File {0} not found!", FilePath));
            Log.AddError(exception, exception.Message);
            throw exception;
        }

        DirectDownload(FilePath);
    }

    internal void DirectDownload(string filePath)
    {
        var context = JJHttpContext.GetInstance();
        context.Response.Redirect(GetDownloadUrl(filePath));
    }

    internal static string GetFileUrl(string filePath, string fileType)
    {
        var appPath = HttpContext.Current!.Request.ApplicationPath;

        if (!appPath.EndsWith("/"))
            appPath += "/";

        var culture = CultureInfo.CurrentCulture.Name + "/";
        return $"{appPath}{culture}MasterData/File/{fileType}?filePath={Cript.Cript64(filePath)}";
    }

    internal static string GetDownloadUrl(string filePath)
    {
        return GetFileUrl(filePath, "Download");
    }

    internal static string GetImageUrl(string filePath)
    {
        return GetFileUrl(filePath, "Image");
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
            throw new JJMasterDataException(Translate.Key("Invalid file path or badly formatted URL"));

        var download = new JJDownloadFile
        {
            FilePath = filePath,
            IsExternalLink = isExternalLink
        };

        return download.GetHtmlBuilder();
    }
}