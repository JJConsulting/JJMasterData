using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Web;
using Microsoft.Extensions.Localization;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using JJMasterData.Core.DataManager.Expressions.Abstractions;
using JJMasterData.Core.UI.Components.Actions;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components.FormView;

internal class ActionsScripts
{
    private IExpressionsService ExpressionsService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }

    public ActionsScripts(
        IExpressionsService expressionsService,
        IDataDictionaryRepository dataDictionaryRepository,
        JJMasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        ExpressionsService = expressionsService;
        DataDictionaryRepository = dataDictionaryRepository;
        StringLocalizer = stringLocalizer;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
    }


    public async Task<string> GetInternalUrlScriptAsync(InternalAction action, IDictionary<string, object> formValues)
    {
        var elementRedirect = action.ElementRedirect;
        var formElement = await DataDictionaryRepository.GetMetadataAsync(action.ElementRedirect.ElementNameRedirect);
        string popUpTitle = formElement.Title;
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];
        string popup = "true";
        int popupSize = (int)elementRedirect.ModalSize;

        var @params = new StringBuilder();

        @params.Append("formname=");
        @params.Append(elementRedirect.ElementNameRedirect);
        @params.Append("&viewtype=");
        @params.Append((int)elementRedirect.ViewType);

        foreach (var r in elementRedirect.RelationFields)
        {
            if (formValues.TryGetValue(r.InternalField, out var value))
            {
                @params.Append("&");
                @params.Append(r.RedirectField);
                @params.Append("=");
                @params.Append(value);
            }
        }
        string url = UrlHelper.GetUrl("Index", "InternalRedirect","MasterData",
            new
            {
                parameters = EncryptionService.EncryptStringWithUrlEscape(@params.ToString())
            });

        return $"ActionManager.executeRedirectAction('{url}',{popup},'{popUpTitle}','{confirmationMessage}','{popupSize}');";
    }


    public string GetUrlRedirectScript(
        UrlRedirectAction action,
        ActionContext actionContext,
        ActionSource actionSource
    )
    {
        var actionMap = actionContext.ToActionMap(action.Name, actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];

        var routeContext = RouteContext.FromFormElement(actionContext.FormElement, ComponentContext.UrlRedirect);

        var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);
        
        return
            $"ActionManager.executeRedirectAction('{actionContext.ParentComponentName}','{encryptedRouteContext}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }

    public string GetFormActionScript(BasicAction action, ActionContext actionContext, ActionSource actionSource)
    {
        var formElement = actionContext.FormElement;
        var actionMap = actionContext.ToActionMap(action.Name, actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];
        
        var actionData = new ActionData
        {
            ComponentName = actionContext.ParentComponentName,
            EncryptedActionMap = encryptedActionMap,
            ConfirmationMessage = confirmationMessage.IsNullOrEmpty() ? null : confirmationMessage
        };
        
        if (actionContext.IsModal)
        {
            var modalRouteContext = RouteContext.FromFormElement(formElement, ComponentContext.Modal);
            var gridViewRouteContext = RouteContext.FromFormElement(formElement, ComponentContext.GridViewReload);

            actionData.ModalTitle = actionContext.FormElement.Title;
            actionData.EncryptedModalRouteContext =
                EncryptionService.EncryptRouteContext(modalRouteContext);
            actionData.EncryptedGridRouteContext =
                EncryptionService.EncryptRouteContext(gridViewRouteContext);
        }

        var actionDataJson = actionData.ToJson();

        var encodedFunction= HttpUtility.HtmlAttributeEncode($"ActionManager.executeAction('{actionDataJson}')");
        
        return encodedFunction;
    }
    

    internal async Task<string> GetUserActionScriptAsync(
        UserCreatedAction userCreatedAction,
        ActionContext actionContext,
        ActionSource actionSource)
    {

        var formStateData = actionContext.FormStateData;

        switch (userCreatedAction)
        {
            case UrlRedirectAction urlRedirectAction:
                return GetUrlRedirectScript(urlRedirectAction, actionContext, actionSource);
            case SqlCommandAction:
                return GetCommandScript(userCreatedAction, actionContext, actionSource);
            case ScriptAction jsAction:
                return ExpressionsService.ParseExpression(jsAction.OnClientClick, formStateData, false);
            case InternalAction internalAction:
                return await GetInternalUrlScriptAsync(internalAction, formStateData.FormValues);
            default:
                return string.Empty;
        }
    }

    public string GetCommandScript(BasicAction action, ActionContext actionContext, ActionSource actionSource)
    {
        var actionMap = actionContext.ToActionMap(action.Name, actionSource);
        string encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];

        return
            $"ActionManager.executeGridAction('{actionContext.ParentComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }
}