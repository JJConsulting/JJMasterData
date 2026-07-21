#nullable disable warnings
using System.Collections.Generic;
using System.Text;
using System.Web;
using JJConsulting.Html.Bootstrap.Components;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.Routing;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Web.Components;

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
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage ?? string.Empty], actionContext.FormStateData);
        int popupSize = (int)elementRedirect.ModalSize;

        var @params = new StringBuilder();

        @params.Append("formname=");
        @params.Append(elementRedirect.ElementNameRedirect);
        @params.Append("&showTitle=");
        @params.Append(action.ShowTitle ? '1' : '0');
        @params.Append("&parentElementName=");
        @params.Append(actionContext.FormElement.Name);
        @params.Append("&viewtype=");
        @params.Append((int)elementRedirect.ViewType);
        @params.Append("&openInModal=");
        @params.Append(elementRedirect.ShowAsModal ? '1' : '0');
        
        foreach (var field in elementRedirect.RelationFields)
        {
            if (actionContext.FormStateData.UserValues?.TryGetValue(field.InternalField, out var userValue) is true)
            {
                @params.Append('&');
                @params.Append(field.RedirectField);
                @params.Append('=');
                @params.Append(userValue);
            }
            
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
                parameters = encryptionService.EncryptString(@params.ToString())
            });

        return
            $"ActionHelper.executeInternalRedirect('{url}','{popupSize}','{confirmationMessage}', '{actionContext.ParentComponentName}', {elementRedirect.ShowAsModal.ToString().ToLowerInvariant()});";
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
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage ?? string.Empty], actionContext.FormStateData);

        return
            $"ActionHelper.executeHTMLTemplate('{actionContext.ParentComponentName}','{stringLocalizer[action.Text ?? string.Empty]}','{encryptedActionMap}','{encryptedRouteContext}',{(string.IsNullOrEmpty(confirmationMessage) ? "''" : $"'{confirmationMessage}'")});";
    }

    private string GetUrlRedirectScript(
        UrlRedirectAction action,
        ActionContext actionContext,
        ActionSource actionSource
    )
    {
        var confirmationMessage =
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage ?? string.Empty], actionContext.FormStateData);

        var isOpenNewTabPage = action.OpenInNewTab ? "true" : "false";
        
        if (actionSource is ActionSource.Field or ActionSource.FormToolbar)
        {
            var actionMap = actionContext.ToActionMap(actionSource);
            var encryptedActionMap = encryptionService.EncryptObject(actionMap);

            var routeContext = RouteContext.FromFormElement(actionContext.FormElement, ComponentContext.UrlRedirect);

            var encryptedRouteContext = encryptionService.EncryptObject(routeContext);

            return
                $"ActionHelper.executeRedirectAction('{actionContext.ParentComponentName}','{encryptedRouteContext}','{encryptedActionMap}', {isOpenNewTabPage} {(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }

        var script = new StringBuilder();
        var url = HttpUtility.JavaScriptStringEncode(urlRedirectService.GetParsedUrl(action, actionContext.FormStateData));
        var isModal = action.IsModal ? "true" : "false";
        var isIframe = action.IsIframe ? "true" : "false";

        var modalTitle = HttpUtility.JavaScriptStringEncode(
            urlRedirectService.GetParsedModalTitle(action, actionContext.FormStateData));

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
        script.Append(confirmationMessage);
        script.Append("');");

        return script.ToString();
    }

    public void AddFormAction(JJLinkButton button, ActionContext actionContext, ActionSource actionSource, bool isAtModal = false)
    {
        var attributes = GetFormActionAttributes(actionContext, actionSource, isAtModal);
        
        foreach (var attr in attributes)
        {
            button.Attributes[attr.Key] = attr.Value;
        }
        
        button.OnClientClick = $"ActionHelper.executeAction('{actionContext.Id}')";
    }
    
    public Dictionary<string,string> GetFormActionAttributes(
        ActionContext actionContext, 
        ActionSource actionSource, 
        bool isAtModal = false)
    {
        var formElement = actionContext.FormElement;
        var action = actionContext.Action;
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = encryptionService.EncryptObject(actionMap);
        var confirmationMessage =
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage ?? string.Empty], actionContext.FormStateData);
    
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

        var attributes = new Dictionary<string,string>
        {
            ["data-component-name"] = actionData.ComponentName,
            ["data-action-map"] = actionData.EncryptedActionMap,
            ["data-is-submit"] = actionData.IsSubmit ? "true" : "false",
            ["data-is-modal"] = actionData.IsModal ? "true" : "false"
        };

        if (actionData.ModalTitle != null)
            attributes["data-modal-title"] = actionData.ModalTitle;

        if (actionData.EncryptedGridViewRouteContext != null)
            attributes["data-grid-view-route-context"] = actionData.EncryptedGridViewRouteContext;

        if (actionData.ConfirmationMessage != null)
            attributes["data-confirmation-message"] = actionData.ConfirmationMessage;

        return attributes;
    }

    private string GetGridRouteContext(FormElement formElement)
    {
        var gridRouteContext = RouteContext.FromFormElement(formElement, ComponentContext.GridViewReload);
        var encryptedRouteContext = encryptionService.EncryptObject(gridRouteContext);
        return encryptedRouteContext;
    }

    internal void AddUserAction(
        JJLinkButton button,
        ActionContext actionContext,
        ActionSource actionSource)
    {
        var formStateData = actionContext.FormStateData;

        var action = actionContext.Action;

        //todo: esses caras poderiam seguir o mesmo padrão do AddFormAction pra despoluir o JS
        if (action is UrlRedirectAction urlRedirectAction)
            button.OnClientClick= GetUrlRedirectScript(urlRedirectAction, actionContext, actionSource);
        else if (action is SqlCommandAction)
            button.OnClientClick= GetSqlCommandScript(actionContext, actionSource);
        else if (action is ScriptAction jsAction)
            button.OnClientClick= expressionsService.ReplaceExpressionWithParsedValues(jsAction.OnClientClick, formStateData) ??
                                  string.Empty;
        else if (action is InternalAction internalAction)
            button.OnClientClick= GetInternalUrlScript(internalAction, actionContext);
        else if (action is HtmlTemplateAction)
            button.OnClientClick= GetHtmlTemplateScript(actionContext, actionSource);
        else
            AddFormAction(button, actionContext, actionSource);
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
            GetParsedConfirmationMessage(stringLocalizer[action.ConfirmationMessage ?? string.Empty], actionContext.FormStateData);

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
