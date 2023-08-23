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
using JJMasterData.Core.DataManager.Expressions.Abstractions;

namespace JJMasterData.Core.UI.Components.FormView;

internal class ActionsScripts
{
    internal IExpressionsService ExpressionsService { get; }
    private IDataDictionaryRepository DataDictionaryRepository { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    internal IEncryptionService EncryptionService { get; }

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
        int popupSize = (int)elementRedirect.PopupSize;

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

        if (actionContext.IsExternalRoute)
        {
            var encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(actionContext.FormElement.Name);
            var url = UrlHelper.GetUrl("GetUrlRedirect", "UrlRedirect","MasterData", new { dictionaryName = encryptedDictionaryName, componentName = actionContext.ParentComponentName });
            return $"ActionManager.executeRedirectAction('{url}','{actionContext.ParentComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")})";
        }

        return
            $"ActionManager.executeRedirectActionAtSamePage('{actionContext.ParentComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }

    public string GetFormActionScript(BasicAction action, ActionContext actionContext, ActionSource actionSource, bool isPopup = false)
    {
        var formElement = actionContext.FormElement;
        var actionMap = actionContext.ToActionMap(action.Name, actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];

        string functionSignature;
        if (isPopup)
        {
            var url = GetFormViewUrl(formElement.Name, action, actionMap);
            functionSignature =
                $"ActionManager.executeFormActionAsPopUp('{url}','{actionContext.ParentComponentName}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }
        else
        {
            functionSignature =
                $"ActionManager.executeFormAction('{actionContext.ParentComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }

        return functionSignature;
    }


    private string GetFormViewUrl(string dictionaryName, BasicAction action, ActionMap actionMap)
    {
        string encryptedDictionaryName = EncryptionService.EncryptStringWithUrlEscape(dictionaryName);


        var pageState = action switch
        {
            InsertAction => PageState.Insert,
            ViewAction => PageState.View,
            _ => PageState.Update
        };

        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        return UrlHelper.GetUrl("GetFormView", "Form","MasterData", new
        {
            dictionaryName = encryptedDictionaryName,
            actionMap = encryptedActionMap,
            pageState
        });
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
            $"JJView.executeGridAction('{actionContext.ParentComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }

    public string GetRefreshScript(ActionContext actionContext)
    {
        string name = actionContext.ParentComponentName;
        if (actionContext.IsExternalRoute)
        {
            string dictionaryNameEncrypted = EncryptionService.EncryptString(actionContext.FormElement.Name);
            return UrlHelper.GetUrl("GetGridViewTable", "Grid","MasterData",  new { dictionaryName = dictionaryNameEncrypted });
        }
        return $"JJView.refresh('{name}', true)";
    }
}