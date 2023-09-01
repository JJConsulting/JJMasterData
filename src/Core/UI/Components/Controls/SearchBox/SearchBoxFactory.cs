#nullable enable
using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.UI.Components.Controls;

namespace JJMasterData.Core.Web.Factories;

internal class SearchBoxFactory : IDynamicControlFactory<JJSearchBox>
{
    private IDataItemService DataItemService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IFormValuesService FormValuesService { get; }
    private IHttpContext HttpContext { get; }
    private IEncryptionService EncryptionService { get; }

    public SearchBoxFactory(
        IDataItemService dataItemService,
        IDataDictionaryRepository dataDictionaryRepository,
        IFormValuesService formValuesService,
        IHttpContext httpContext,
        IEncryptionService encryptionService)
    {
        DataItemService = dataItemService;
        DataDictionaryRepository = dataDictionaryRepository;
        FormValuesService = formValuesService;
        HttpContext = httpContext;
        EncryptionService = encryptionService;
    }

    public JJSearchBox Create()
    {
        return new JJSearchBox(HttpContext, EncryptionService, DataItemService);
    }

    public JJSearchBox Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        if (field.DataItem == null)
            throw new ArgumentNullException(nameof(field.DataItem));

        var search = new JJSearchBox(HttpContext, EncryptionService, DataItemService)
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
    

    public JJSearchBox Create(FormElement formElement, IDictionary<string,object?> values, IDictionary<string,object?> userValues)
    {
        var elementName = HttpContext.Request.QueryString["elementName"];
        var fieldName = HttpContext.Request.QueryString["fieldName"];
        var pageState = (PageState)int.Parse(HttpContext.Request.QueryString["pageState"]);

        var field = formElement.Fields[fieldName];
        var expOptions = new FormStateData(values, userValues, pageState);

        var searchBox = Create(formElement, field, new(expOptions, elementName));
        return searchBox;
    }
}