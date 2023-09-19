#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataManager.Services;

public class LookupService : ILookupService
{
    private IFormValues FormValues { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IEntityRepository EntityRepository { get; }
    private IExpressionsService ExpressionsService { get; }
    private IEncryptionService EncryptionService { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }

    public LookupService(
        IFormValues formValues,
        IDataDictionaryRepository dataDictionaryRepository,
        IEntityRepository entityRepository,
        IExpressionsService expressionsService,
        IEncryptionService encryptionService,
        JJMasterDataUrlHelper urlHelper)
    {
        FormValues = formValues;
        DataDictionaryRepository = dataDictionaryRepository;
        EntityRepository = entityRepository;
        ExpressionsService = expressionsService;
        EncryptionService = encryptionService;
        UrlHelper = urlHelper;
    }

    
    public string GetFormViewUrl(DataElementMap elementMap, FormStateData formStateData, string componentName)
    {
        var lookupParameters = new LookupParameters(elementMap.ElementName, componentName, elementMap.FieldKey,elementMap.FieldDescription,
            elementMap.EnableElementActions, elementMap.Filters);

        var encryptedLookupParameters =
            EncryptionService.EncryptStringWithUrlEscape(lookupParameters.ToQueryString(ExpressionsService, formStateData));
        
        return UrlHelper.GetUrl("Index", "Lookup", "MasterData",new { lookupParameters = encryptedLookupParameters });
    }

    public string GetDescriptionUrl(string elementName, string fieldName, string componentName, PageState pageState)
    {
        return UrlHelper.GetUrl("GetDescription", "Lookup","MasterData", 
            new
            {
                elementName = EncryptionService.EncryptStringWithUrlEscape(elementName),
                componentName,
                fieldName,
                pageState
            });
    }

    public async Task<string?> GetDescriptionAsync(
        DataElementMap elementMap,
        FormStateData formStateData,
        object? value,
        bool allowOnlyNumbers)
    {
        if (string.IsNullOrEmpty(value?.ToString()))
            return null;

        if (elementMap.Filters == null)
            return null;

        if (allowOnlyNumbers)
        {
            bool isNumeric = int.TryParse(value?.ToString(), out _);
            if (!isNumeric)
                return null;
        }

        var filters = GetFilters(elementMap, value, formStateData);

        IDictionary<string, object?> fields;
        
        try
        {
            fields = await GetFieldsAsync(elementMap, filters);
        }
        catch
        {
            return null;
        }


        if (string.IsNullOrEmpty(elementMap.FieldDescription))
            return fields[elementMap.FieldKey]?.ToString();

        return fields.Any() ? fields[elementMap.FieldDescription]?.ToString() : null;
    }

    private IDictionary<string, object> GetFilters(DataElementMap elementMap, object? value, FormStateData formStateData)
    {
        var filters = new Dictionary<string, object>();

        if (elementMap.Filters.Count > 0)
        {
            foreach (var filter in elementMap.Filters)
            {
                string? filterParsed =
                    ExpressionsService.ParseExpression(filter.Value?.ToString(), formStateData, false);
                filters[filter.Key] = StringManager.ClearText(filterParsed);
            }
        }

        filters[elementMap.FieldKey] = StringManager.ClearText(value?.ToString());
        return filters;
    }

    private async Task<IDictionary<string, object?>> GetFieldsAsync(DataElementMap elementMap, IDictionary<string, object> filters)
    {
        var formElement = await DataDictionaryRepository.GetMetadataAsync(elementMap.ElementName);
        return await EntityRepository.GetFieldsAsync(formElement, filters);
    }
    
    public string? GetSelectedValue(string componentName)
    {
        return FormValues[componentName];
    }
}