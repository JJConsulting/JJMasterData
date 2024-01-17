using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Repository.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataManager.Services;


public class UrlRedirectService(IEntityRepository entityRepository, FormValuesService formValuesService, ExpressionsService expressionsService)
{
    private IEntityRepository EntityRepository { get; } = entityRepository;
    private FormValuesService FormValuesService { get; } = formValuesService;
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    
    public async Task<JsonComponentResult> GetUrlRedirectResult(
        JJDataPanel dataPanel,
        ActionMap actionMap)
    {
        var urlRedirectAction = actionMap.GetAction<UrlRedirectAction>(dataPanel.FormElement);

        IDictionary<string,object> dbValues;

        if (actionMap.PkFieldValues.Any())
        {
            dbValues = await EntityRepository.GetFieldsAsync(dataPanel.FormElement, actionMap.PkFieldValues);
        }
        else
        {
            dbValues = new Dictionary<string, object>();
        }
           
        var values = await FormValuesService.GetFormValuesWithMergedValuesAsync(dataPanel.FormElement,new FormStateData(dbValues,dataPanel.UserValues,dataPanel.PageState), true, dataPanel.FieldNamePrefix);
        
        DataHelper.CopyIntoDictionary(values, actionMap.PkFieldValues);
        
        return GetJsonResult(values, urlRedirectAction);
    }

    public async Task<JsonComponentResult> GetUrlRedirectResult(
        JJGridView gridView,
        ActionMap actionMap)
    {
        var urlRedirectAction = actionMap.GetAction<UrlRedirectAction>(gridView.FormElement);

        var values = await FormValuesService.GetFormValuesWithMergedValuesAsync(gridView.FormElement,new FormStateData(new Dictionary<string, object>(),gridView.UserValues,PageState.List), true);
        
        DataHelper.CopyIntoDictionary(values, actionMap.PkFieldValues);
        
        return GetJsonResult(values, urlRedirectAction);
    }

    private JsonComponentResult GetJsonResult(Dictionary<string, object> values, UrlRedirectAction urlRedirectAction)
    {
        var formStateData = new FormStateData(values, PageState.List);
        var parsedUrl = ExpressionsService.ReplaceExpressionWithParsedValues(System.Web.HttpUtility.UrlDecode(urlRedirectAction.UrlRedirect), formStateData);
        var parsedTitle =  ExpressionsService.ReplaceExpressionWithParsedValues(urlRedirectAction.ModalTitle, formStateData);
        var model = new UrlRedirectModel
        {
            IsIframe = urlRedirectAction.IsIframe,
            UrlRedirect = parsedUrl!,
            ModalTitle = parsedTitle!,
            UrlAsModal = urlRedirectAction.IsModal,
            ModalSize = urlRedirectAction.ModalSize
        };
        
        return new JsonComponentResult(model);
    }
}