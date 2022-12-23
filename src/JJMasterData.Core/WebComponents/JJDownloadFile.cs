using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Logging;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Html;
using System;
using System.Globalization;
using System.IO;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.WebComponents;

public class JJDownloadFile : JJBaseView
{
    public const string DirectDownloadParameter = "jjdirectdownload";
    public const string DownloadParameter = "jjdownload";
    
    internal IHttpContext HttpContext { get; }

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
    
    public JJDownloadFile(IHttpContext httpContext)
    {
        HttpContext = httpContext;
    }

    public JJDownloadFile(string filePath, IHttpContext httpContext)
    {
        FilePath = filePath;
        HttpContext = httpContext;
    }

    private HtmlBuilder GetDownloadHtmlElement()
    {
        var file = new FileInfo(FilePath);
        string fileName = file.Name;
        string size = Format.FormatFileSize(file.Length);
        string lastWriteTime = file.LastWriteTime.ToDateTimeString();
        string url = HttpContext.Request.AbsoluteUri.Replace(DirectDownloadParameter, DownloadParameter);

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
        HttpContext.Response.Redirect(GetDownloadUrl(filePath,HttpContext));
    }

    internal static string GetDownloadUrl(string filePath, IHttpContext httpContext)
    {
        var appPath = httpContext.Request.ApplicationPath;

        if (!appPath.EndsWith("/"))
            appPath += "/";

        var culture = CultureInfo.CurrentCulture.Name + "/";
        return $"{appPath}{culture}MasterData/File/Download?filePath={Cript.Cript64(filePath)}";
    }

    public static bool IsDownloadRoute(IHttpContext httpContext)
    {
        if (httpContext.Request.QueryString(DirectDownloadParameter) != null)
            return true;
        if (httpContext.Request.QueryString(DownloadParameter) != null)
            return true;
        return false;
    }

    public static HtmlBuilder ResponseRoute(IHttpContext httpContext)
    {
        bool isExternalLink = false;
        string criptFilePath = httpContext.Request.QueryString(DownloadParameter);
        if (criptFilePath == null)
        {
            criptFilePath = httpContext.Request.QueryString(DirectDownloadParameter);
            isExternalLink = true;
        }

        if (criptFilePath == null)
            return null;

        string filePath = Cript.Descript64(criptFilePath);
        if (filePath == null)
            throw new JJMasterDataException(Translate.Key("Invalid file path or badly formatted URL"));

        var download = new JJDownloadFile(httpContext)
        {
            FilePath = filePath,
            IsExternalLink = isExternalLink
        };

        return download.GetHtmlBuilder();
    }


    
}
