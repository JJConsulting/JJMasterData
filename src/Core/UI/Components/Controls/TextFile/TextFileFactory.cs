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

namespace JJMasterData.Core.Web.Factories;

internal class TextFileFactory : IControlFactory<JJTextFile>
{
    private IHttpContext HttpContext { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IComponentFactory<JJUploadView> UploadViewFactory { get; }
    private IControlFactory<JJTextGroup> TextBoxFactory { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextFileFactory(
        IHttpContext httpContext,
        JJMasterDataUrlHelper urlHelper,
        IComponentFactory<JJUploadView> uploadViewFactory,
        IControlFactory<JJTextGroup>  textBoxFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        UrlHelper = urlHelper;
        UploadViewFactory = uploadViewFactory;
        TextBoxFactory = textBoxFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }
    
    
    public JJTextFile Create()
    {
        return new JJTextFile(HttpContext,UrlHelper, UploadViewFactory, TextBoxFactory,EncryptionService, StringLocalizer);
    }

    public JJTextFile Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var (formStateData, parentName, value) = context;

        if (field == null)
            throw new ArgumentNullException(nameof(field));

        if (field.DataFile == null)
            throw new ArgumentException("DataFile cannot be null");

        var text = Create();
        text.FormElementField = field;
        text.PageState = formStateData.PageState;
        text.Text = value != null ? value.ToString() : "";
        text.FormValues = formStateData.FormValues;
        text.Name = field.Name;

        text.Attributes.Add("panelName", parentName);
        text.UserValues = formStateData.UserValues;
        text.FormElement = formElement;

        text.SetAttr(field.Attributes);

        return text;
    }
}