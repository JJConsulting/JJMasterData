using System;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class TextFileFactory(IHttpRequest request,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJTextFile>
{
    private IHttpRequest Request { get; } = request;
    private IComponentFactory ComponentFactory { get; } = componentFactory;

    private IEncryptionService EncryptionService { get; } = encryptionService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;


    public JJTextFile Create()
    {
        return new JJTextFile(Request,ComponentFactory, StringLocalizer, EncryptionService);
    }

    public JJTextFile Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var (formStateData, _, value) = context;

        if (field == null)
            throw new ArgumentNullException(nameof(field));

        if (field.DataFile == null)
            throw new ArgumentException("DataFile cannot be null");

        var textFile = Create();
        textFile.FormElementField = field;
        textFile.PageState = formStateData.PageState;
        textFile.Text = value != null ? value.ToString() : "";
        textFile.FormStateValues = formStateData.Values;
        textFile.Name = field.Name;
        textFile.FieldName = field.Name;
        textFile.Enabled = true;
        textFile.UserValues = formStateData.UserValues;
        textFile.FormElement = formElement;

        textFile.SetAttr(field.Attributes);

        return textFile;
    }
}