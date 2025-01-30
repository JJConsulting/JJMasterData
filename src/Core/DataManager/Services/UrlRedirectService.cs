using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataManager.Services;


public class UrlRedirectService(
    IHttpRequest httpRequest,
    IEntityRepository entityRepository,
    FormValuesService formValuesService,
    ExpressionsService expressionsService)
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
        var parsedTitle =  expressionsService.ReplaceExpressionWithParsedValues(action.ModalTitle, formStateData);
        
        var model = new UrlRedirectModel
        {
            IsIframe = action.IsIframe,
            UrlRedirect = parsedUrl,
            ModalTitle = parsedTitle!,
            UrlAsModal = action.IsModal,
            ModalSize = action.ModalSize,
            OpenNewTabPage = action.OpenInNewTab
        };
        
        return new JsonComponentResult(model);
    }

    public string GetParsedUrl(UrlRedirectAction action, FormStateData formStateData)
    {
        var formStateDataCopy = formStateData.DeepCopy();
        
        formStateDataCopy.Values.Add("AppPath", httpRequest.ApplicationPath);
        
        var decodedUrl = HttpUtility.UrlDecode(action.UrlRedirect);
        
        return expressionsService.ReplaceExpressionWithParsedValues(decodedUrl, formStateDataCopy, action.EncryptParameters);
    }
}