using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

public class FormUploadFactory
{
    public ILoggerFactory LoggerFactory { get; }

    private JJMasterDataEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    //This prevents a circular dependency:
    //GridViewFactory depends on FieldControlFactory
    //FieldControlFactory depends on TextFileFactory
    //TextFileFactory depends on FormUploadFactory
    //FormUploadFactory depends on GridViewFactory !Recursive!
    private Lazy<GridViewFactory> GridViewFactory { get; }

    private UploadAreaFactory UploadAreaFactory { get; }

    private FileDownloaderFactory FileDownloaderFactory { get; }

    private IHttpContext CurrentContext { get; }
    private TextGroupFactory TextGroupFactory { get; }

    public FormUploadFactory(
        IHttpContext currentContext,
        TextGroupFactory textGroupFactory,
        FileDownloaderFactory fileDownloaderFactory,
        UploadAreaFactory uploadAreaFactory,
        Lazy<GridViewFactory> gridViewFactory,
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        TextGroupFactory = textGroupFactory;
        FileDownloaderFactory = fileDownloaderFactory;
        UploadAreaFactory = uploadAreaFactory;
        GridViewFactory = gridViewFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

    public JJFormUpload CreateFormUpload()
    {
        return new JJFormUpload(
            CurrentContext, 
            FileDownloaderFactory, 
            TextGroupFactory, 
            UploadAreaFactory,
            GridViewFactory,
            EncryptionService, 
            StringLocalizer,
            LoggerFactory.CreateLogger<JJFormUpload>());
    }
}