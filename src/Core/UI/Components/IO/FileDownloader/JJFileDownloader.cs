using System;
using System.IO;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Components;

public class JJFileDownloader : HtmlComponent
{
    public const string DirectDownloadParameter = "jjdirectdownload";
    public const string DownloadParameter = "jjdownload";
    
    public string FilePath { get; set; }
    public bool IsExternalLink { get; set; }
    internal IHttpContext CurrentContext { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal JJMasterDataUrlHelper UrlHelper { get; }
    internal ILogger<JJFileDownloader> Logger { get; }
    internal IEncryptionService EncryptionService { get; }
    
    public JJFileDownloader(
        IHttpContext currentContext,
        JJMasterDataUrlHelper urlHelper, 
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILogger<JJFileDownloader> logger)
    {
        CurrentContext = currentContext;
        StringLocalizer = stringLocalizer;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
        Logger = logger;
    }


    internal override HtmlBuilder BuildHtml()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new JJMasterDataException(StringLocalizer["Invalid file path or badly formatted URL"]);
    
        if (IsExternalLink)
            return GetDownloadHtmlElement();
        
        GetDirectDownloadRedirect();

        return null;
    }

    private HtmlBuilder GetDownloadHtmlElement()
    {
        var file = new FileInfo(FilePath);
        string fileName = file.Name;
        string size = Format.FormatFileSize(file.Length);
        string lastWriteTime = file.LastWriteTime.ToDateTimeString();
        string url = CurrentContext.Request.AbsoluteUri.Replace(DirectDownloadParameter, DownloadParameter);

        var htmlTitle = new JJTitle();
        htmlTitle.Title = StringLocalizer["Downloading"];
        htmlTitle.SubTitle = fileName.ToLower();
        
        var html = new HtmlBuilder(HtmlTag.Div)
            .AppendComponent(htmlTitle)
            .Append(HtmlTag.Section, section =>
            {
                section.WithCssClass("container mt-3");
                section.Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass("jumbotron px-3 py-4 px-sm-4 py-sm-5 bg-light rounded-3 mb-3");
                    div.Append(HtmlTag.Div, div =>
                    {
                        div.Append(HtmlTag.H1, h1 =>
                        {
                            h1.AppendComponent(new JJIcon("fa fa-cloud-download text-info"));
                            h1.AppendText(fileName);
                        });
                        div.Append(HtmlTag.P, p =>
                        {
                            p.AppendText($"{StringLocalizer["File Size:"]} {size}");
                            p.Append(HtmlTag.Br);
                            p.AppendText($"{StringLocalizer["Last write time:"]} {lastWriteTime}");
                        });
                        div.Append(HtmlTag.Hr, hr =>
                        {
                            hr.WithCssClass("my-4");
                        });
                        div.Append(HtmlTag.P, p =>
                        {
                            p.AppendText(StringLocalizer["You are downloading file {0}.", fileName]);
                            p.AppendText(" ");
                            p.AppendText($"{StringLocalizer["If the download not start automatically"]}, ");
                            p.Append(HtmlTag.A, a =>
                            {
                                a.WithAttribute("href", url);
                                a.AppendText(StringLocalizer["click here."]);
                            });
                        });
                    });
                });
            });
        return html;
    }
    
    internal RedirectComponentResult GetDirectDownloadRedirect()
    {
        if (string.IsNullOrEmpty(FilePath))
            throw new ArgumentNullException(nameof(FilePath));

        if (!File.Exists(FilePath))
        {
            var exception = new JJMasterDataException(StringLocalizer["File {0} not found!", FilePath]);
            Logger.LogError(exception, "File {FilePath} not found!", FilePath);
            throw exception;
        }

        return GetDirectDownloadRedirect(FilePath);
    }

    internal RedirectComponentResult GetDirectDownloadRedirect(string filePath)
    {
        return new RedirectComponentResult(GetDownloadUrl(filePath));
    }

    internal string GetDownloadUrl(string filePath)
    {
        var encryptedFilePath = EncryptionService.EncryptStringWithUrlEscape(filePath);

        return UrlHelper.GetUrl("Download", "File", "MasterData", new {filePath = encryptedFilePath});
    }
    
    internal string GetDownloadUrl(JJMasterDataUrlHelper urlHelper,string filePath)
    {
        var encryptedFilePath = EncryptionService.EncryptStringWithUrlEscape(filePath);

        return urlHelper.GetUrl("Download", "File", "MasterData", new {filePath = encryptedFilePath});
    }
    
    public RedirectComponentResult GetDirectDownloadFromUrl()
    {
        bool isExternalLink = false;
        string criptFilePath = CurrentContext.Request.QueryString[DownloadParameter];
        if (criptFilePath == null)
        {
            criptFilePath = CurrentContext.Request.QueryString[DirectDownloadParameter];
            isExternalLink = true;
        }

        if (criptFilePath == null)
            throw new JJMasterDataException("Invalid file path or badly formatted URL");

        string filePath = EncryptionService.DecryptStringWithUrlUnescape(criptFilePath);

        FilePath = filePath ?? throw new JJMasterDataException("Invalid file path or badly formatted URL");
        IsExternalLink = isExternalLink;

        return GetDirectDownloadRedirect();
    }


}
