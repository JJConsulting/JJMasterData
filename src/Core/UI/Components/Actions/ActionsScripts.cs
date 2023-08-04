﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components.Widgets;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.FormView;

internal class ActionsScripts
{
    internal IExpressionsService ExpressionsService { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    private JJMasterDataUrlHelper UrlHelper { get; }
    internal JJMasterDataEncryptionService EncryptionService { get; }
    
    public ActionsScripts(
        IExpressionsService expressionsService,
        JJMasterDataUrlHelper urlHelper,
        JJMasterDataEncryptionService encryptionService, 
        IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        ExpressionsService = expressionsService;
        StringLocalizer = stringLocalizer;
        UrlHelper = urlHelper;
        EncryptionService = encryptionService;
    }


    public string GetInternalUrlScript(InternalAction action, IDictionary<string, dynamic> formValues)
    {
        var elementRedirect = action.ElementRedirect;
        var dicRepository = JJService.Provider.GetScopedDependentService<IDataDictionaryRepository>();
        var formElement = dicRepository.GetMetadata(action.ElementRedirect.ElementNameRedirect);
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
        string url = UrlHelper.GetUrl("Index", "InternalRedirect",
            new
            {
                parameters = EncryptionService.EncryptStringWithUrlEscape(@params.ToString()),
                Area = "MasterData"
            });

        return $"ActionManager.executeRedirectAction('{url}',{popup},'{popUpTitle}','{confirmationMessage}','{popupSize}');";
    }
    

    public string GetUrlRedirectScript(
        UrlRedirectAction action,
        ActionContext actionContext,
        ActionSource actionSource
    )
    {
        var actionMap = actionContext.ToActionMap(action.Name,actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage ?? string.Empty];

        return
            $"ActionManager.executeRedirectAction('{actionContext.ParentComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";

    }

    public string GetFormActionScript(BasicAction action, ActionContext actionContext, ActionSource actionSource, bool isPopup = false)
    {
        var formElement = actionContext.FormElement;
        var actionMap = actionContext.ToActionMap(action.Name,actionSource);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage ?? string.Empty];

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
        var encryptionService = JJService.Provider.GetService<JJMasterDataEncryptionService>();
        string encryptedDictionaryName = encryptionService.EncryptStringWithUrlEscape(dictionaryName);


        var pageState = action switch
        {
            InsertAction => PageState.Insert,
            ViewAction => PageState.View,
            _ => PageState.Update
        };
        
        var encryptedActionMap = encryptionService.EncryptActionMap(actionMap);
        return UrlHelper.GetUrl("GetFormView", "Form", new
        {
            dictionaryName = encryptedDictionaryName,
            actionMap = encryptedActionMap,
            pageState,
            Area = "MasterData"
        });
    }
    
    internal string GetUserActionScript(
        UserCreatedAction userCreatedAction,
        ActionContext actionContext,
        ActionSource actionSource)
    {

        var formStateData = actionContext.FormStateData;
        
        switch (userCreatedAction)
        {
            case UrlRedirectAction urlRedirectAction:
                return GetUrlRedirectScript(urlRedirectAction, actionContext,actionSource);
            case SqlCommandAction:
                return GetCommandScript(userCreatedAction,actionContext,actionSource);
            case ScriptAction jsAction:
                return ExpressionsService.ParseExpression(jsAction.OnClientClick, formStateData.PageState, false, formStateData.FormValues, formStateData.UserValues);
            case InternalAction internalAction:
                return GetInternalUrlScript(internalAction, formStateData.FormValues);
            default:
                return string.Empty;
        }
    }

    public string GetCommandScript(BasicAction action, ActionContext actionContext, ActionSource actionSource)
    {
        var actionMap = actionContext.ToActionMap(action.Name,actionSource);
        string encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];

        return
            $"JJView.executeGridAction('{actionContext.ParentComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }
}