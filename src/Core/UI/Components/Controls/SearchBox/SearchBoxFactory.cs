#nullable enable
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.Web.Factories;

internal class SearchBoxFactory : IControlFactory<JJSearchBox>
{
    private IDataItemService DataItemService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IFormValuesService FormValuesService { get; }
    private IHttpRequest HttpRequest { get; }
    private IEncryptionService EncryptionService { get; }

    public SearchBoxFactory(
        IDataItemService dataItemService,
        IDataDictionaryRepository dataDictionaryRepository,
        IFormValuesService formValuesService,
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