#nullable enable
using System;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class SearchBoxFactory : IControlFactory<JJSearchBox>
{
    private DataItemService DataItemService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private FormValuesService FormValuesService { get; }
    private IHttpRequest HttpRequest { get; }
    private IEncryptionService EncryptionService { get; }

    public SearchBoxFactory(
        DataItemService dataItemService,
        IDataDictionaryRepository dataDictionaryRepository,
        FormValuesService formValuesService,
        IHttpRequest httpRequest,
        IEncryptionService encryptionService)
    {
        DataItemService = dataItemService;
        DataDictionaryRepository = dataDictionaryRepository;
        FormValuesService = formValuesService;
        HttpRequest = httpRequest;
        EncryptionService = encryptionService;
    }

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