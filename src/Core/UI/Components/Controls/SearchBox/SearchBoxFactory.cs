#nullable enable
using System;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class SearchBoxFactory(DataItemService dataItemService,
        IDataDictionaryRepository dataDictionaryRepository,
        FormValuesService formValuesService,
        IHttpRequest httpRequest,
        IEncryptionService encryptionService)
    : IControlFactory<JJSearchBox>
{
    private DataItemService DataItemService { get; } = dataItemService;
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private FormValuesService FormValuesService { get; } = formValuesService;
    private IHttpRequest HttpRequest { get; } = httpRequest;
    private IEncryptionService EncryptionService { get; } = encryptionService;

    public JJSearchBox Create()
    {
        return new JJSearchBox(HttpRequest, EncryptionService, DataItemService);
    }

    public JJSearchBox Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        if (field.DataItem == null)
            throw new ArgumentNullException(nameof(field.DataItem));

        var search = new JJSearchBox(HttpRequest, EncryptionService, DataItemService)
        {
            DataItem = field.DataItem,
            Name = field.Name,
            FieldName = field.Name,
            ParentElementName = formElement.ParentName,
            ElementName = formElement.Name,
            Visible = true,
            AutoReloadFormFields = false,
            FormStateData = controlContext.FormStateData,
            UserValues = controlContext.FormStateData.UserValues
        };

        if (controlContext.Value != null)
            search.SelectedValue = controlContext.Value.ToString()!;

        return search;
    }
    
}