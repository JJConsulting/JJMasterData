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
    private JJMasterDataUrlHelper UrlHelper { get; }

    public SearchBoxFactory(
        IDataItemService dataItemService,
        IDataDictionaryRepository dataDictionaryRepository,
        IFormValuesService formValuesService,
        IHttpContext httpContext,
        IEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper)
    {
        DataItemService = dataItemService;
        DataDictionaryRepository = dataDictionaryRepository;
        FormValuesService = formValuesService;
        HttpContext = httpContext;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
    }

    public JJSearchBox Create()
    {
        return new JJSearchBox(HttpContext, EncryptionService, DataItemService, UrlHelper);
    }

    public JJSearchBox Create(FormElement formElement, FormElementField field, ControlContext controlContext)
    {
        if (field.DataItem == null)
            throw new ArgumentNullException(nameof(field.DataItem));

        var search = new JJSearchBox(HttpContext, EncryptionService, DataItemService, UrlHelper)
        {
            DataItem = field.DataItem,
            Name = field.Name,
            FieldName = field.Name,
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

    public async Task<JJSearchBox?> CreateAsync(string? dictionaryName, string fieldName, PageState pageState, IDictionary<string, object?> userValues)
    {
        if (string.IsNullOrEmpty(dictionaryName))
            return null;

        IDictionary<string, object?>? formValues = null;
        var formElement = await DataDictionaryRepository.GetMetadataAsync(dictionaryName);
        var dataItem = formElement.Fields[fieldName].DataItem;
        if (dataItem == null)
            throw new ArgumentNullException(nameof(dataItem));

        if (dataItem.HasSqlExpression())
        {
            formValues = await FormValuesService.GetFormValuesWithMergedValuesAsync(formElement, pageState, true);
        }

        var field = formElement.Fields[fieldName];
        var expOptions = new FormStateData(userValues, formValues, pageState);
        return Create(formElement, field, new(expOptions, null, null));
    }

    public JJSearchBox? CreateIfExists(FormElement formElement, IDictionary<string,object?> values, IDictionary<string,object?> userValues)
    {
        var elementName = HttpContext.Request.QueryString("dictionaryName");
        var fieldName = HttpContext.Request.QueryString("fieldName");
        var pageState = (PageState)int.Parse(HttpContext.Request.QueryString("pageState"));

        if (!formElement.Name.Equals(elementName))
            return null;

        var field = formElement.Fields[fieldName];
        var expOptions = new FormStateData(values, userValues, pageState);

        var searchBox = Create(formElement, field, new(expOptions, formElement.Name, elementName));
        return searchBox;
    }
}