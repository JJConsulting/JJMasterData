using System;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Factories;

public class TextFileFactory
{
    private IHttpContext HttpContext { get; }
    private FormUploadFactory FormUploadFactory { get; }
    private TextGroupFactory TextGroupFactory { get; }
    private JJMasterDataEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextFileFactory(
        IHttpContext httpContext,
        FormUploadFactory formUploadFactory,
        TextGroupFactory textGroupFactory,
        JJMasterDataEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        HttpContext = httpContext;
        FormUploadFactory = formUploadFactory;
        TextGroupFactory = textGroupFactory;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }

    public JJTextFile CreateTextFile()
    {
        return new JJTextFile(HttpContext, FormUploadFactory, TextGroupFactory,EncryptionService, StringLocalizer);
    }
    
    internal JJTextFile CreateTextFile(FormElement formElement,
        FormElementField field, ExpressionOptions expOptions, object value, string panelName)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        if (field.DataFile == null)
            throw new ArgumentException("DataFile cannot be null");

        var text = CreateTextFile();
        text.ElementField = field;
        text.PageState = expOptions.PageState;
        text.Text = value != null ? value.ToString() : "";
        text.FormValues = expOptions.FormValues;
        text.Name = field.Name;

        text.Attributes.Add("pnlname", panelName);
        text.UserValues = expOptions.UserValues;
        text.FormElement = formElement;

        text.SetAttr(field.Attributes);

        return text;
    }
}