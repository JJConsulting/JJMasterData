using System.Text;
using System.Web;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class ActionScripts(
    ExpressionsService expressionsService,
    UrlRedirectService urlRedirectService,
    IUrlHelper urlHelper,
    IEncryptionService encryptionService,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private string GetInternalUrlScript(InternalAction action, ActionContext actionContext)
    {
        var elementRedirect = action.ElementRedirect;
        string confirmationMessage =
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);
        int popupSize = (int)elementRedirect.ModalSize;

        var @params = new StringBuilder();

        @params.Append("formname=");
        @params.Append(elementRedirect.ElementNameRedirect);
        @params.Append("&showTitle=");
        @params.Append(action.ShowTitle ? '1' : '0');
        @params.Append("&viewtype=");
        @params.Append((int)elementRedirect.ViewType);

        foreach (var field in elementRedirect.RelationFields)
        {
            if (actionContext.FormStateData.Values.TryGetValue(field.InternalField, out var value))
            {
                @params.Append('&');
                @params.Append(field.RedirectField);
                @params.Append('=');
                @params.Append(value);
            }
        }

        string url = urlHelper.Action("Index", "InternalRedirect",
            new
            {
                Area = "MasterData",
                parameters = encryptionService.EncryptStringWithUrlEscape(@params.ToString())
            });

        return
            $"ActionHelper.executeInternalRedirect('{url}','{popupSize}','{confirmationMessage}');";
    }

    
    private string GetHtmlTemplateScript(
        ActionContext actionContext,
        ActionSource actionSource
    )
    {
        var action = actionContext.Action;
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = encryptionService.EncryptObject(actionMap);

        var encryptedRouteContext =
            encryptionService.EncryptObject(RouteContext.FromFormElement(actionContext.FormElement,
                ComponentContext.FormViewReload));

        var confirmationMessage =
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);

        return
            $"ActionHelper.executeHTMLTemplate('{actionContext.ParentComponentName}','{stringLocalizer[action.Text]}','{encryptedActionMap}','{encryptedRouteContext}',{(string.IsNullOrEmpty(confirmationMessage) ? "''" : $"'{confirmationMessage}'")});";
    }

    private string GetUrlRedirectScript(
        UrlRedirectAction action,
        ActionContext actionContext,
        ActionSource actionSource
    )
    {
        string confirmationMessage =
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);

        if (actionSource is ActionSource.Field or ActionSource.FormToolbar)
        {
            var actionMap = actionContext.ToActionMap(actionSource);
            var encryptedActionMap = encryptionService.EncryptObject(actionMap);

            var routeContext = RouteContext.FromFormElement(actionContext.FormElement, ComponentContext.UrlRedirect);

            var encryptedRouteContext = encryptionService.EncryptObject(routeContext);

            return
                $"ActionHelper.executeRedirectAction('{actionContext.ParentComponentName}','{encryptedRouteContext}','{encryptedActionMap}', '{action.OpenInNewTab.ToString().ToLower()}' {(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }

        var script = new StringBuilder();
        string url = urlRedirectService.GetParsedUrl(action, actionContext.FormStateData);
        string isModal = action.IsModal ? "true" : "false";
        string isIframe = action.IsIframe ? "true" : "false";
        string isOpenNewTabPage = action.OpenInNewTab ? "true" : "false";

        string modalTitle = action.ModalTitle;

        script.Append("ActionHelper.executeClientSideRedirect('");
        script.Append(url);
        script.Append("',");
        script.Append(isModal);
        script.Append(",'");
        script.Append(modalTitle);
        script.Append("','");
        script.Append((int)action.ModalSize);
        script.Append("',");
        script.Append(isIframe);
        script.Append(',');
        script.Append(isOpenNewTabPage);
        script.Append(",'");
        script.Append(stringLocalizer[action.ConfirmationMessage]);
        script.Append("');");

        return script.ToString();
    }

    public string GetFormActionScript(
        ActionContext actionContext, 
        ActionSource actionSource, 
        bool encode = true, 
        bool isAtModal = false)
    {
        var formElement = actionContext.FormElement;
        var action = actionContext.Action;
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = encryptionService.EncryptObject(actionMap);
        var confirmationMessage =
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);
        
        var actionData = new ActionData
        {
            ComponentName = actionContext.ParentComponentName,
            EncryptedActionMap = encryptedActionMap,
            IsSubmit = actionContext.IsSubmit,
            ConfirmationMessage = string.IsNullOrEmpty(confirmationMessage) ? null : confirmationMessage
        };
        
        if (action is IModalAction { ShowAsModal: true } modalAction)
        {
            actionData.ModalTitle = modalAction.ModalTitle ?? string.Empty;
            actionData.IsModal = true;
        }
        else if (isAtModal)
        {
            actionData.IsModal = true;
        }

        if (actionData.IsModal && !actionData.IsSubmit)
            actionData.EncryptedGridViewRouteContext = GetGridRouteContext(formElement);
        
        var actionDataJson = actionData.ToJson();

        var functionSignature = $"ActionHelper.executeAction('{actionDataJson}');";

        if (encode)
            return HttpUtility.HtmlAttributeEncode(functionSignature);

        return functionSignature;
    }

    private string GetGridRouteContext(FormElement formElement)
    {
        var gridRouteContext = RouteContext.FromFormElement(formElement, ComponentContext.GridViewReload);
        var encryptedRouteContext = encryptionService.EncryptObject(gridRouteContext);
        return encryptedRouteContext;
    }

    internal string GetUserActionScript(
        ActionContext actionContext,
        ActionSource actionSource)
    {
        var formStateData = actionContext.FormStateData;

        var action = actionContext.Action;

        return action switch
        {
            UrlRedirectAction urlRedirectAction => GetUrlRedirectScript(urlRedirectAction, actionContext, actionSource),
            SqlCommandAction => GetSqlCommandScript(actionContext, actionSource),
            ScriptAction jsAction => HttpUtility.HtmlAttributeEncode(
                expressionsService.ReplaceExpressionWithParsedValues(jsAction.OnClientClick, formStateData) ??
                string.Empty),
            InternalAction internalAction => GetInternalUrlScript(internalAction, actionContext),
            HtmlTemplateAction  => GetHtmlTemplateScript(actionContext, actionSource),
            _ => GetFormActionScript(actionContext, actionSource)
        };
    }


    private string GetSqlCommandScript(ActionContext actionContext, ActionSource actionSource)
    {
        var action = actionContext.Action;
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = encryptionService.EncryptObject(actionMap);

        var encryptedRouteContext =
            encryptionService.EncryptObject(RouteContext.FromFormElement(actionContext.FormElement,
                ComponentContext.FormViewReload));

        var confirmationMessage =
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);

        return
            $"ActionHelper.executeSqlCommand('{actionContext.ParentComponentName}','{encryptedActionMap}','{encryptedRouteContext}', {(actionContext.IsSubmit ? "true" : "false")},{(string.IsNullOrEmpty(confirmationMessage) ? "''" : $"'{confirmationMessage}'")});";
    }

    public static string GetHideModalScript(string componentName) => $"ActionHelper.hideActionModal('{componentName}')";

    private string GetParsedConfirmationMessage(string originalMessage,
        FormStateData formStateData)
    {
        return expressionsService.ReplaceExpressionWithParsedValues(originalMessage, formStateData);
    }
}