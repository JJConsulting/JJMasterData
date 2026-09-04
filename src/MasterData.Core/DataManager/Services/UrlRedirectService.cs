#nullable disable warnings
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Commons.Security;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.DataManager.Services;


public class UrlRedirectService(
    IHttpContextAccessor httpRequest,
    IEntityRepository entityRepository,
    IStringLocalizer<MasterDataResources> localizer,
    FormValuesService formValuesService,
    ExpressionsService expressionsService,
    HmacHelper hmacHelper)
{
    public async Task<JsonComponentResult> GetUrlRedirectResult(
        JJDataPanel dataPanel,
        ActionMap actionMap)
    {
        var urlRedirectAction = actionMap.GetAction<UrlRedirectAction>(dataPanel.FormElement);

        Dictionary<string,object> dbValues;

        if (actionMap.PkFieldValues.Count > 0)
        {
            dbValues = await entityRepository.GetFieldsAsync(dataPanel.FormElement, actionMap.PkFieldValues);
        }
        else
        {
            dbValues = new Dictionary<string, object>();
        }
           
        var values = await formValuesService.GetFormValuesWithMergedValuesAsync(dataPanel.FormElement,new FormStateData(dbValues,dataPanel.UserValues,dataPanel.PageState), true, dataPanel.FieldNamePrefix);
        
        DataHelper.CopyIntoDictionary(values, actionMap.PkFieldValues);
        
        return GetJsonResult(values, urlRedirectAction);
    }

    public async Task<JsonComponentResult> GetUrlRedirectResult(
        JJGridView gridView,
        ActionMap actionMap)
    {
        var urlRedirectAction = actionMap.GetAction<UrlRedirectAction>(gridView.FormElement);

        var values = await formValuesService.GetFormValuesWithMergedValuesAsync(gridView.FormElement,new FormStateData(new Dictionary<string, object>(),gridView.UserValues,PageState.List), true);
        
        DataHelper.CopyIntoDictionary(values, actionMap.PkFieldValues);
        
        return GetJsonResult(values, urlRedirectAction);
    }

    private JsonComponentResult GetJsonResult(Dictionary<string, object> values, UrlRedirectAction action)
    {
        var formStateData = new FormStateData(values, PageState.List);
        var parsedUrl = GetParsedUrl(action, formStateData);
        var parsedTitle = GetParsedModalTitle(action, formStateData);
        
        var model = new UrlRedirectModel
        {
            IsIframe = action.IsIframe,
            UrlRedirect = parsedUrl,
            ModalTitle = parsedTitle!,
            UrlAsModal = action.IsModal,
            ModalSize = action.ModalSize,
            OpenInNewTab = action.OpenInNewTab
        };
        
        return new JsonComponentResult(model);
    }

    public string GetParsedUrl(UrlRedirectAction action, FormStateData formStateData)
    {
        var formStateDataCopy = formStateData.DeepCopy();
        
        formStateDataCopy.Values.Add("AppPath", httpRequest.HttpContext?.Request.GetApplicationPath());
        
        var decodedUrl = HttpUtility.UrlDecode(action.UrlRedirect);
        
        var parsedUrl = expressionsService.ReplaceExpressionWithParsedValues(
            decodedUrl,
            formStateDataCopy,
            action.EncryptParameters) ?? string.Empty;

        return action.SignParametersWithHmac ? SignUrlParameters(parsedUrl) : parsedUrl;
    }

    private string SignUrlParameters(string url)
    {
        var fragmentIndex = url.IndexOf('#');
        var fragment = fragmentIndex >= 0 ? url[fragmentIndex..] : string.Empty;
        var urlWithoutFragment = fragmentIndex >= 0 ? url[..fragmentIndex] : url;
        var questionMarkIndex = urlWithoutFragment.IndexOf('?');

        if (questionMarkIndex < 0)
            return url;

        var urlPath = urlWithoutFragment[..questionMarkIndex];
        var pathToSign = Uri.TryCreate(urlPath, UriKind.Absolute, out var absoluteUri)
            ? absoluteUri.AbsolutePath
            : urlPath;
        var queryPairs = QueryHelpers.ParseQuery(urlWithoutFragment[questionMarkIndex..])
            .Where(pair => !pair.Key.Equals("signature", StringComparison.OrdinalIgnoreCase))
            .SelectMany(pair => pair.Value.Select(value => new KeyValuePair<string, string?>(pair.Key, value)))
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ThenBy(pair => pair.Value, StringComparer.Ordinal)
            .ToList();

        if (queryPairs.Count == 0)
            return url;

        var content = QueryHelpers.AddQueryString(pathToSign, queryPairs);
        var signature = hmacHelper.Generate(content);
        var signedUrl = QueryHelpers.AddQueryString(
            urlPath,
            queryPairs.Append(new KeyValuePair<string, string?>("signature", signature)));

        return $"{signedUrl}{fragment}";
    }

    public string GetParsedModalTitle(UrlRedirectAction action, FormStateData formStateData)
    {
        return expressionsService.ReplaceExpressionWithParsedValues(localizer[action.ModalTitle], formStateData) ?? string.Empty;
    }
}
