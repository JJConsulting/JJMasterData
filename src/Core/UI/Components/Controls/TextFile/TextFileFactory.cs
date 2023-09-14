using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

internal class TextFileFactory : IControlFactory<JJTextFile>
{
    private IHttpRequest Request { get; }
    private IComponentFactory<JJUploadView> UploadViewFactory { get; }
    private IControlFactory<JJTextGroup> TextBoxFactory { get; }
    private IComponentFactory<JJFileDownloader> FileDownloaderFactory { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextFileFactory(
        IHttpRequest request,
        IComponentFactory<JJUploadView> uploadViewFactory,
        IControlFactory<JJTextGroup>  textBoxFactory,
        IComponentFactory<JJFileDownloader> fileDownloaderFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        Request = request;
        UploadViewFactory = uploadViewFactory;
        TextBoxFactory = textBoxFactory;
        FileDownloaderFactory = fileDownloaderFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }
    
    
    public JJTextFile Create()
    {
        return new JJTextFile(Request,UploadViewFactory, TextBoxFactory, StringLocalizer, FileDownloaderFactory,EncryptionService);
    }

    public JJTextFile Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var (formStateData, value) = context;

        if (field == null)
            throw new ArgumentNullException(nameof(field));

        if (field.DataFile == null)
            throw new ArgumentException("DataFile cannot be null");

        var textFile = Create();
        textFile.FormElementField = field;
        textFile.PageState = formStateData.PageState;
        textFile.Text = value != null ? value.ToString() : "";
        textFile.FortStateValues = formStateData.FormValues;
        textFile.Name = field.Name;
        textFile.FieldName = field.Name;
        textFile.Enabled = true;
        textFile.UserValues = formStateData.UserValues;
        textFile.FormElement = formElement;

        textFile.SetAttr(field.Attributes);

        return textFile;
    }
}