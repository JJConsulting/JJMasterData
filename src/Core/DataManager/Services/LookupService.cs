#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataManager.Services;

public class LookupService(IFormValues formValues,
    IDataDictionaryRepository dataDictionaryRepository,
    IEntityRepository entityRepository,
    ExpressionsService expressionsService,
    IEncryptionService encryptionService,
    ElementMapService elementMapService,
    MasterDataUrlHelper urlHelper)
{
    private IFormValues FormValues { get; } = formValues;
    private IDataDictionaryRepository DataDictionaryRepository { get; } = dataDictionaryRepository;
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private ElementMapService ElementMapService { get; } = elementMapService;
    private MasterDataUrlHelper UrlHelper { get; } = urlHelper;


    public string GetFormViewUrl(DataElementMap elementMap, FormStateData formStateData, string componentName)
    {
        var lookupParameters = new LookupParameters(elementMap.ElementName, componentName, elementMap.FieldId,elementMap.FieldDescription,
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

        IDictionary<string, object?> fields;
        
        try
        {
            fields = await ElementMapService.GetFieldsAsync(elementMap, value, formStateData);
        }
        catch
        {
            return null;
        }


        if (string.IsNullOrEmpty(elementMap.FieldDescription))
            return fields[elementMap.FieldId]?.ToString();

        if (elementMap.FieldDescription != null)
            return fields.Any() ? fields[elementMap.FieldDescription]?.ToString() : null;

        return null;
    }
    
    public string? GetSelectedValue(string componentName)
    {
        return FormValues[componentName];
    }
}