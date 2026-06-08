using System;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.IO.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class TextFileFactory(
        IHttpContextAccessor request,
        IComponentFactory componentFactory,
        IEncryptionService encryptionService,
        IFileStorage fileStorage,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : IControlFactory<JJTextFile>
{
    public JJTextFile Create()
    {
        return new JJTextFile(request,componentFactory, fileStorage, stringLocalizer, encryptionService);
    }

    public JJTextFile Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var formStateData = context.FormStateData;
        var value = context.Value;
        
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

        textFile.SetAttributes(field.Attributes);

        return textFile;
    }
}
