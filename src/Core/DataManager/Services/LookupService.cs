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
    ExpressionsService expressionsService,
    IEncryptionService encryptionService,
    ElementMapService elementMapService,
    IMasterDataUrlHelper urlHelper)
{
    private IFormValues FormValues { get; } = formValues;
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    private IEncryptionService EncryptionService { get; } = encryptionService;
    private ElementMapService ElementMapService { get; } = elementMapService;
    private IMasterDataUrlHelper UrlHelper { get; } = urlHelper;


    public string GetFormViewUrl(DataElementMap elementMap, FormStateData? formStateData, string componentName)
    {
        var lookupParameters = new LookupParameters(elementMap.ElementName, componentName, elementMap.IdFieldName,elementMap.DescriptionFieldName,
            elementMap.EnableElementActions, elementMap.Filters);

        var encryptedLookupParameters =
            EncryptionService.EncryptStringWithUrlEscape(lookupParameters.ToQueryString(ExpressionsService, formStateData));
        
        return UrlHelper.Action("Index", "Lookup", new { Area = "MasterData", lookupParameters = encryptedLookupParameters });
    }

    public async Task<string?> GetDescriptionAsync(
        DataElementMap elementMap,
        FormStateData? formStateData,
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

        Dictionary<string, object?> values;
        
        try
        {
            values = await ElementMapService.GetFieldsAsync(elementMap, value, formStateData);
        }
        catch
        {
            return null;
        }


        if (string.IsNullOrEmpty(elementMap.DescriptionFieldName) && values.TryGetValue(elementMap.IdFieldName, out var id))
            return id?.ToString();

        if (elementMap.DescriptionFieldName != null && values.TryGetValue(elementMap.DescriptionFieldName, out var description))
            return description?.ToString();

        return null;
    }
    
    public string? GetSelectedValue(string componentName)
    {
        return FormValues[componentName];
    }
}