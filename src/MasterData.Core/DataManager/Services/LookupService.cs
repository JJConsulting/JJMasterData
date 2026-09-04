#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.Routing;

namespace JJMasterData.Core.DataManager.Services;

public class LookupService(
    IHttpContextAccessor httpContextAccessor,
    ExpressionsService expressionsService,
    DataProtectionService encryptionService,
    ElementMapService elementMapService,
    LinkGenerator linkGenerator)
{
    public string GetFormViewUrl(DataElementMap elementMap, FormStateData? formStateData, string componentName)
    {
        var lookupParameters = new LookupParameters(elementMap.ElementName, componentName, elementMap.IdFieldName,
            elementMap.DescriptionFieldName,
            elementMap.EnableElementActions, elementMap.Filters);

        var encryptedLookupParameters =
            encryptionService.Protect(
                lookupParameters.ToQueryString(expressionsService, formStateData));

        var httpContext = httpContextAccessor.HttpContext ??
                          throw new InvalidOperationException(
                              "Lookup URLs can only be generated during an HTTP request.");
        return linkGenerator.GetPathByAction(
                   httpContext,
                   "Index",
                   "Lookup",
                   new { Area = "MasterData", lookupParameters = encryptedLookupParameters }) ??
               throw new InvalidOperationException("Unable to generate the lookup URL.");
    }

    public async Task<string?> GetDescriptionAsync(
        DataElementMap elementMap,
        FormStateData? formStateData,
        object? value,
        bool allowOnlyNumbers)
    {
        if (string.IsNullOrEmpty(value?.ToString()))
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
            values = await elementMapService.GetFieldsAsync(elementMap, value, formStateData);
        }
        catch
        {
            return null;
        }


        if (string.IsNullOrEmpty(elementMap.DescriptionFieldName) &&
            values.TryGetValue(elementMap.IdFieldName, out var id))
            return id?.ToString();

        if (elementMap.DescriptionFieldName != null &&
            values.TryGetValue(elementMap.DescriptionFieldName, out var description))
            return description?.ToString();

        return null;
    }

    public string? GetSelectedValue(string componentName)
    {
        return httpContextAccessor.HttpContext?.Request.GetFormValue(componentName);
    }
}
