using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Web.Components;
using JJMasterData.Web.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JJMasterData.Web.DataManager.Services;

public sealed class LookupRequestService(
    IHttpContextAccessor httpContextAccessor,
    ExpressionsService expressionsService,
    IEncryptionService encryptionService,
    IUrlHelper urlHelper)
{
    public string GetFormViewUrl(DataElementMap elementMap, FormStateData? formStateData, string componentName)
    {
        var parameters = new LookupParameters(elementMap.ElementName, componentName, elementMap.IdFieldName,
            elementMap.DescriptionFieldName ?? string.Empty, elementMap.EnableElementActions, elementMap.Filters);
        formStateData ??= new FormStateData(new Dictionary<string, object?>(), PageState.List);
        var encrypted = encryptionService.EncryptString(parameters.ToQueryString(expressionsService, formStateData));
        return urlHelper.Action("Index", "Lookup", new { Area = "MasterData", lookupParameters = encrypted })!;
    }

    public string? GetSelectedValue(string componentName) =>
        httpContextAccessor.HttpContext?.Request.GetFormValue(componentName);
}
