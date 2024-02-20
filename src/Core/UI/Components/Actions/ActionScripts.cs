using System.Text;
using System.Web;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Expressions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

namespace JJMasterData.Core.UI.Components;

public class ActionScripts(
    ExpressionsService expressionsService,
    IMasterDataUrlHelper urlHelper,
    IEncryptionService encryptionService,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private ExpressionsService ExpressionsService { get; } = expressionsService;
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;
    private IMasterDataUrlHelper UrlHelper { get; } = urlHelper;
    private IEncryptionService EncryptionService { get; } = encryptionService;

    private string GetInternalUrlScript(InternalAction action, ActionContext actionContext)
    {
        var elementRedirect = action.ElementRedirect;
        string confirmationMessage =
            GetParsedConfirmationMessage(StringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);
        int popupSize = (int)elementRedirect.ModalSize;

        var @params = new StringBuilder();

        @params.Append("formname=");
        @params.Append(elementRedirect.ElementNameRedirect);
        @params.Append("&viewtype=");
        @params.Append((int)elementRedirect.ViewType);

        foreach (var field in elementRedirect.RelationFields)
        {
            if (actionContext.FormStateData.Values.TryGetValue(field.InternalField, out var value))
            {
                @params.Append("&");
                @params.Append(field.RedirectField);
                @params.Append("=");
                @params.Append(value);
            }
        }

        string url = UrlHelper.Action("Index", "InternalRedirect",
            new
            {
                Area = "MasterData",
                parameters = EncryptionService.EncryptStringWithUrlEscape(@params.ToString())
            });

        return
            $"ActionHelper.executeInternalRedirect('{url}','{popupSize}','{confirmationMessage}');";
    }


    private string GetUrlRedirectScript(
        UrlRedirectAction action,
        ActionContext actionContext,
        ActionSource actionSource
    )
    {
        string confirmationMessage =
            GetParsedConfirmationMessage(StringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);

        if (actionSource is ActionSource.Field or ActionSource.FormToolbar)
        {
            var actionMap = actionContext.ToActionMap(actionSource);
            var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);

            var routeContext = RouteContext.FromFormElement(actionContext.FormElement, ComponentContext.UrlRedirect);

            var encryptedRouteContext = EncryptionService.EncryptRouteContext(routeContext);

            return
                $"ActionHelper.executeRedirectAction('{actionContext.ParentComponentName}','{encryptedRouteContext}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }

        var script = new StringBuilder();
        string url = ExpressionsService.ReplaceExpressionWithParsedValues(HttpUtility.UrlDecode(action.UrlRedirect),
            actionContext.FormStateData);
        string isModal = action.IsModal ? "true" : "false";
        string isIframe = action.IsIframe ? "true" : "false";
        string modalTitle = action.ModalTitle;

        script.Append("ActionHelper.doUrlRedirect('");
        script.Append(url);
        script.Append("',");
        script.Append(isModal);
        script.Append(",'");
        script.Append(modalTitle);
        script.Append("','");
        script.Append((int)action.ModalSize);
        script.Append("',");
        script.Append(isIframe);
        script.Append(",'");
        script.Append(StringLocalizer[action.ConfirmationMessage]);
        script.Append("');");

        return script.ToString();
    }

    public string GetFormActionScript(ActionContext actionContext, ActionSource actionSource, bool encode = true)
    {
        var formElement = actionContext.FormElement;
        var action = actionContext.Action;
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage =
            GetParsedConfirmationMessage(StringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);
        
        var actionData = new ActionData
        {
            ComponentName = actionContext.ParentComponentName,
            EncryptedActionMap = encryptedActionMap,
            ConfirmationMessage = confirmationMessage.IsNullOrEmpty() ? null : confirmationMessage
        };

        if (action is IModalAction { ShowAsModal: true } modalAction)
        {
            actionData.ModalTitle = modalAction.ModalTitle ?? string.Empty;
            actionData.IsModal = true;
        }
        
        var formViewRouteContext = RouteContext.FromFormElement(formElement, ComponentContext.FormViewReload);
        actionData.EncryptedFormViewRouteContext = EncryptionService.EncryptRouteContext(formViewRouteContext);
        actionData.IsSubmit = actionContext.IsSubmit;

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
            ScriptAction jsAction => HttpUtility.HtmlAttributeEncode(
                ExpressionsService.ReplaceExpressionWithParsedValues(jsAction.OnClientClick, formStateData) ??
                string.Empty),
            InternalAction internalAction => GetInternalUrlScript(internalAction, actionContext),
            _ => GetFormActionScript(actionContext, actionSource)
        };
    }


    private string GetSqlCommandScript(ActionContext actionContext, ActionSource actionSource)
    {
        var action = actionContext.Action;
        var actionMap = actionContext.ToActionMap(actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);

        var encryptedRouteContext =
            EncryptionService.EncryptRouteContext(RouteContext.FromFormElement(actionContext.FormElement,
                ComponentContext.FormViewReload));

        var confirmationMessage =
            GetParsedConfirmationMessage(StringLocalizer[action.ConfirmationMessage], actionContext.FormStateData);

        return
            $"ActionHelper.executeSqlCommand('{actionContext.ParentComponentName}','{encryptedActionMap}','{encryptedRouteContext}', {(actionContext.IsSubmit ? "true" : "false")},{(string.IsNullOrEmpty(confirmationMessage) ? "''" : $"'{confirmationMessage}'")});";
    }

    public static string GetHideModalScript(string componentName) => $"ActionHelper.hideActionModal('{componentName}')";

    private string GetParsedConfirmationMessage(string originalMessage,
        FormStateData formStateData)
    {
        return ExpressionsService.ReplaceExpressionWithParsedValues(originalMessage, formStateData);
    }
}