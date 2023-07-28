using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;

namespace JJMasterData.Core.Web.Factories;

internal class FormUploadFactory : IComponentFactory<JJFormUpload>
{
    public ILoggerFactory LoggerFactory { get; }

    private JJMasterDataEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    //This prevents a circular dependency:
    //GridViewFactory depends on ControlsFactory
    //ControlsFactory depends on TextFileFactory
    //TextFileFactory depends on FormUploadFactory
    //FormUploadFactory depends on GridViewFactory !Recursive!
    private Lazy< IFormElementComponentFactory<JJGridView>> GridViewFactory { get; }

    private IComponentFactory<JJUploadArea> UploadAreaFactory { get; }

    private IComponentFactory<JJFileDownloader> FileDownloaderFactory { get; }

    private IHttpContext CurrentContext { get; }
    private IControlFactory<JJTextGroup> TextBoxFactory { get; }

    public FormUploadFactory(
        IHttpContext currentContext,
        IControlFactory<JJTextGroup>  textBoxFactory,
        IComponentFactory<JJFileDownloader> fileDownloaderFactory,
        IComponentFactory<JJUploadArea> uploadAreaFactory,
        Lazy< IFormElementComponentFactory<JJGridView>> gridViewFactory,
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer,
        ILoggerFactory loggerFactory)
    {
        CurrentContext = currentContext;
        TextBoxFactory = textBoxFactory;
        FileDownloaderFactory = fileDownloaderFactory;
        UploadAreaFactory = uploadAreaFactory;
        GridViewFactory = gridViewFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
        LoggerFactory = loggerFactory;
    }

    public JJFormUpload Create()
    {
        return new JJFormUpload(
            CurrentContext, 
            FileDownloaderFactory, 
            TextBoxFactory, 
            UploadAreaFactory,
            GridViewFactory,
            EncryptionService, 
            StringLocalizer,
            LoggerFactory);
    }
}