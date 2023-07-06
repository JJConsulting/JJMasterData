﻿using System;
using System.IO;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Extensions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Components;

public class JJFileDownloader : JJBaseView
{
    public const string DirectDownloadParameter = "jjdirectdownload";
    public const string DownloadParameter = "jjdownload";
    
    public required string FilePath { get; set; }

    public bool IsExternalLink { get; set; }
    
    internal IHttpContext CurrentContext { get; }
    
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal JJMasterDataUrlHelper UrlHelper { get; }
    internal ILogger<JJFileDownloader> Logger { get; }
    internal JJMasterDataEncryptionService EncryptionService { get; }
    
    
    public JJFileDownloader(
        IHttpContext currentContext,
        JJMasterDataUrlHelper urlHelper, 
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILogger<JJFileDownloader> logger)
    {
        CurrentContext = currentContext;
        StringLocalizer = stringLocalizer;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
        Logger = logger;
    }


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
                            p.AppendText(StringLocalizer["You are downloading file {0}.", fileName]);
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
            var exception = new JJMasterDataException(StringLocalizer["File {0} not found!", FilePath]);
            Logger.LogError(exception, "File {FilePath} not found!", FilePath);
            throw exception;
        }

        DirectDownload(FilePath);
    }

    internal void DirectDownload(string filePath)
    {
        CurrentContext.Response.Redirect(GetDownloadUrl(filePath));
    }

    internal string GetDownloadUrl(string filePath)
    {
        var encryptedFilePath = EncryptionService.EncryptStringWithUrlEncode(filePath);

        return UrlHelper.GetUrl("Download", "File", new {filePath = encryptedFilePath});
    }


    public static bool IsDownloadRoute()
    {
        var currentContext = JJService.Provider.GetScopedDependentService<IHttpContext>();
        
        if (currentContext.Request.QueryString(DirectDownloadParameter) != null)
            return true;
        if (currentContext.Request.QueryString(DownloadParameter) != null)
            return true;
        return false;
    }

    public static HtmlBuilder ResponseRoute()
    {
        var currentContext = JJService.Provider.GetScopedDependentService<IHttpContext>();
        var encryptionService = JJService.Provider.GetScopedDependentService<JJMasterDataEncryptionService>();
        var factory = JJService.Provider.GetScopedDependentService<FileDownloaderFactory>();
        
        bool isExternalLink = false;
        string criptFilePath = currentContext.Request.QueryString(DownloadParameter);
        if (criptFilePath == null)
        {
            criptFilePath = currentContext.Request.QueryString(DirectDownloadParameter);
            isExternalLink = true;
        }

        if (criptFilePath == null)
            return null;

        string filePath = encryptionService.DecryptStringWithUrlDecode(criptFilePath);
        if (filePath == null)
            throw new JJMasterDataException("Invalid file path or badly formatted URL");

        var download = factory.CreateFileDownloader(filePath);
        download.IsExternalLink = isExternalLink;

        return download.GetHtmlBuilder();
    }


}