using System;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataManager.Services;

public class UrlRedirectService 
{
    private FormValuesService FormValuesService { get; }
    private ExpressionsService ExpressionsService { get; }

    public UrlRedirectService(FormValuesService formValuesService, ExpressionsService expressionsService)
    {
        FormValuesService = formValuesService;
        ExpressionsService = expressionsService;
    }
    
    public async Task<UrlRedirectModel> GetUrlRedirectAsync(FormElement formElement,ActionMap actionMap, PageState pageState)
    {
        var urlAction = actionMap.ActionSource switch
        {
            ActionSource.Field => (UrlRedirectAction)formElement.Fields[actionMap?.FieldName]
                .Actions.Get(actionMap!.ActionName),
            ActionSource.GridTable =>
                (UrlRedirectAction)formElement.Options.GridTableActions.Get(actionMap!.ActionName),
            ActionSource.GridToolbar => (UrlRedirectAction)formElement.Options.GridToolbarActions.Get(actionMap!
                .ActionName),
            ActionSource.FormToolbar => (UrlRedirectAction)formElement.Options.FormToolbarActions.Get(actionMap!
                .ActionName),
            _ => throw new ArgumentOutOfRangeException()
        };

        var values = await FormValuesService.GetFormValuesAsync(formElement,pageState);
        var formData = new FormStateData(values, pageState);
        var parsedUrl = ExpressionsService.ParseExpression(urlAction.UrlRedirect, formData, false);

        var model = new UrlRedirectModel()
        {
            IsIframe = urlAction.IsIframe,
            UrlRedirect = parsedUrl!,
            PopUpTitle = urlAction.ModalTitle,
            UrlAsPopUp = urlAction.IsModal
        };
        return model;
    }
}