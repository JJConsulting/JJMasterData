using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;

namespace JJMasterData.Core.Web.Factories;

internal class TextFileFactory : IControlFactory<JJTextFile>
{
    private IHttpContext HttpContext { get; }
    private IComponentFactory<JJFormUpload> FormUploadFactory { get; }
    private IControlFactory<JJTextGroup> TextBoxFactory { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextFileFactory(
        IHttpContext httpContext,
        IComponentFactory<JJFormUpload> formUploadFactory,
        IControlFactory<JJTextGroup>  textBoxFactory,
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        FormUploadFactory = formUploadFactory;
        TextBoxFactory = textBoxFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }
    
    
    public JJTextFile Create()
    {
        return new JJTextFile(HttpContext, FormUploadFactory, TextBoxFactory,EncryptionService, StringLocalizer);
    }

    public JJTextFile Create(FormElement formElement,FormElementField field, FormStateData formStateData, string parentName, object value)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        if (field.DataFile == null)
            throw new ArgumentException("DataFile cannot be null");

        var text = Create();
        text.ElementField = field;
        text.PageState = formStateData.PageState;
        text.Text = value != null ? value.ToString() : "";
        text.FormValues = formStateData.FormValues;
        text.Name = field.Name;

        text.Attributes.Add("pnlname", parentName);
        text.UserValues = formStateData.UserValues;
        text.FormElement = formElement;

        text.SetAttr(field.Attributes);

        return text;
    }
}