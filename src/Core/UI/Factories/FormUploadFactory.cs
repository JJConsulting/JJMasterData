using System;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Logging;

namespace JJMasterData.Core.Web.Factories;

public class FormUploadFactory
{
    public ILoggerFactory LoggerFactory { get; }

    private JJMasterDataEncryptionService EncryptionService { get; }

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
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        TextGroupFactory = textGroupFactory;
        FileDownloaderFactory = fileDownloaderFactory;
        UploadAreaFactory = uploadAreaFactory;
        GridViewFactory = gridViewFactory;
        EncryptionService = encryptionService;
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
            LoggerFactory.CreateLogger<JJFormUpload>());
    }
}