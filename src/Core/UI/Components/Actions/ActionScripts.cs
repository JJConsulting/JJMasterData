using System.Collections.Generic;
using System.Text;
using System.Web;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

namespace JJMasterData.Core.UI.Components;

public class ActionScripts
{
    private ExpressionsService ExpressionsService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    private IEncryptionService EncryptionService { get; }

    public ActionScripts(
        ExpressionsService expressionsService,
        JJMasterDataUrlHelper urlHelper,
        IEncryptionService encryptionService,
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
    }


    public string GetInternalUrlScript(InternalAction action, IDictionary<string, object> formValues)
    {
        var elementRedirect = action.ElementRedirect;
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];
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

        string url = UrlHelper.GetUrl("Index", "InternalRedirect", "MasterData",
            new
            {
                parameters = EncryptionService.EncryptStringWithUrlEscape(@params.ToString())
            });

        return
            $"ActionHelper.executeInternalRedirect('{url}','{popupSize}','{confirmationMessage}');";
    }


    public string GetUrlRedirectScript(
        UrlRedirectAction action,
        ActionContext actionContext,
        ActionSource actionSource
    )
    {
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];

        var routeContext = RouteContext.FromFormElement(actionContext.FormElement, ComponentContext.UrlRedirect);

        var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);

        return
            $"ActionHelper.executeRedirectAction('{actionContext.ParentComponentName}','{encryptedRouteContext}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }

    public string GetFormActionScript(ActionContext actionContext, ActionSource actionSource, bool encode = true)
    {
        var formElement = actionContext.FormElement;
        var action = actionContext.Action;
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];

        var actionData = new ActionData
        {
            ComponentName = actionContext.ParentComponentName,
            EncryptedActionMap = encryptedActionMap,
            ConfirmationMessage = confirmationMessage.IsNullOrEmpty() ? null : confirmationMessage
        };
        
        var formViewRouteContext = RouteContext.FromFormElement(formElement, ComponentContext.FormViewReload);
        actionData.EncryptedFormViewRouteContext = EncryptionService.EncryptRouteContext(formViewRouteContext);
        actionData.IsSubmit = actionContext.IsSubmit;
        if (actionContext.IsModal)
        {
            var modalRouteContext = RouteContext.FromFormElement(formElement, ComponentContext.Modal);
            var gridViewRouteContext = RouteContext.FromFormElement(formElement, ComponentContext.GridViewReload);
            
            actionData.ModalTitle = actionContext.FormElement.Title;
            actionData.EncryptedGridViewRouteContext = EncryptionService.EncryptRouteContext(gridViewRouteContext);
            actionData.EncryptedModalRouteContext = EncryptionService.EncryptRouteContext(modalRouteContext);
        }

        var actionDataJson = actionData.ToJson();

        var functionSignature = $"ActionHelper.executeAction('{actionDataJson}');";

        if (encode)
            return HttpUtility.HtmlAttributeEncode(functionSignature);

        return functionSignature;
    }
    

    internal string GetUserActionScript(
        ActionContext actionContext,
        ActionSource actionSource)
    {
        var formStateData = actionContext.FormStateData;

        var userCreatedAction = actionContext.Action as UserCreatedAction;
        
        return userCreatedAction switch
        {
            UrlRedirectAction urlRedirectAction => GetUrlRedirectScript(urlRedirectAction, actionContext, actionSource),
            SqlCommandAction => GetSqlCommandScript(actionContext, actionSource),
            ScriptAction jsAction => HttpUtility.HtmlAttributeEncode(ExpressionsService.ParseExpression(jsAction.OnClientClick, formStateData) ?? string.Empty),
            InternalAction internalAction => GetInternalUrlScript(internalAction, formStateData.Values),
            _ => GetFormActionScript(actionContext, actionSource)
        };
    }



    public string GetSqlCommandScript(ActionContext actionContext, ActionSource actionSource)
    {
        var action = actionContext.Action;
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        
        var encryptedRouteContext = EncryptionService.EncryptRouteContext(RouteContext.FromFormElement(actionContext.FormElement, ComponentContext.FormViewReload));
        
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];
        
        return
            $"ActionHelper.executeSqlCommand('{actionContext.ParentComponentName}','{encryptedActionMap}','{encryptedRouteContext}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }

    public static string GetHideModalScript(string componentName)
    {
        return $"ActionHelper.hideActionModal('{componentName}')";
    }
}