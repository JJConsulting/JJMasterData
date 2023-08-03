using System;
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
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.UI.Components.Widgets;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.FormView;

internal class FormViewScripts
{
    internal FormElement FormElement { get; }
    internal IExpressionsService Expression { get; }
    internal string ComponentName { get; }
    internal IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    internal JJMasterDataEncryptionService EncryptionService { get; }

    public FormViewScripts(JJGridView gridView)
    {
        FormElement = gridView.FormElement;
        Expression = gridView.ExpressionsService;
        ComponentName = gridView.Name;
        StringLocalizer = gridView.StringLocalizer;
        EncryptionService = gridView.EncryptionService;
    }

    public FormViewScripts(FormElement formElement,
                           IExpressionsService expression,
                           string componentName,
                           IStringLocalizer<JJMasterDataResources> stringLocalizer,
                           JJMasterDataEncryptionService encryptionService)
    {
        FormElement = formElement;
        Expression = expression;
        ComponentName = componentName;
        StringLocalizer = stringLocalizer;
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

        var urlHelper = JJMasterDataUrlHelper.GetInstance();
        string url = urlHelper.GetUrl("Index", "InternalRedirect",
            new
            {
                parameters = EncryptionService.EncryptStringWithUrlEscape(@params.ToString()),
                Area = "MasterData"
            });

        return $"JJView.executeRedirectAction('{url}',{popup},'{popUpTitle}','{confirmationMessage}','{popupSize}');";
    }


    public string GetUrlRedirectScript(
        UrlRedirectAction action,
        IDictionary<string, dynamic> formValues,
        ActionSource contextAction,
        string fieldName)
    {
        var actionMap = new ActionMap(contextAction, FormElement, formValues, action.Name)
        {
            FieldName = fieldName
        };
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage ?? string.Empty];

        return
            $"JJView.executeRedirectAction('{ComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";

    }


    public string GetFormActionScript(BasicAction action, IDictionary<string, dynamic> formValues,
        ActionSource actionSource, bool isPopup = false)
    {
        var actionMap = new ActionMap(actionSource, FormElement, formValues, action.Name);
        var encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage ?? string.Empty];

        string functionSignature;
        if (isPopup)
        {
            var url = GetFormViewUrl(FormElement.Name, action, actionMap);
            functionSignature =
                $"FormViewScripts.executeFormActionAsPopUp('{url}','{ComponentName}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }
        else
        {
            functionSignature =
                $"FormViewScripts.executeFormAction('{ComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }

        return functionSignature;
    }


    private static string GetFormViewUrl(string dictionaryName, BasicAction action, ActionMap actionMap)
    {
        var encryptionService = JJService.Provider.GetService<JJMasterDataEncryptionService>();
        string encryptedDictionaryName = encryptionService.EncryptStringWithUrlEscape(dictionaryName);


        var pageState = action switch
        {
            InsertAction => PageState.Insert,
            ViewAction => PageState.View,
            _ => PageState.Update
        };

        var urlHelper = JJMasterDataUrlHelper.GetInstance();
        var encryptedActionMap = encryptionService.EncryptActionMap(actionMap);
        return urlHelper.GetUrl("GetFormView", "Form", new
        {
            dictionaryName = encryptedDictionaryName,
            actionMap = encryptedActionMap,
            pageState,
            Area = "MasterData"
        });
    }

    public string GetCommandScript(BasicAction action, IDictionary<string, dynamic> formValues,
        ActionSource contextAction)
    {
        var actionMap = new ActionMap(contextAction, FormElement, formValues, action.Name);
        string encryptedActionMap = EncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];

        return
            $"JJView.executeGridAction('{ComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }

    public async Task<JJLinkButton> GetLinkGridAsync(BasicAction action, FormStateData formStateData)
    {
        return await GetLinkAsync(action, formStateData, ActionSource.GridTable);
    }

    public async Task<JJLinkButton> GetLinkFormToolbarAsync(BasicAction action, FormStateData formStateData)
    {
        return await GetLinkAsync(action, formStateData, ActionSource.FormToolbar);
    }

    public async Task<JJLinkButton> GetLinkFieldAsync(BasicAction action, FormStateData formStateData, string panelName)
    {
        return await GetLinkAsync(action, formStateData, ActionSource.Field, panelName);
    }

    private async Task<JJLinkButton> GetLinkAsync(
        BasicAction action,
        FormStateData formStateData,
        ActionSource contextAction,
        string fieldName = null)
    {
        var linkButtonFactory = new LinkButtonFactory(Expression);
        var pageState = formStateData.PageState;
        var formValues = formStateData.FormValues;
        var link = await linkButtonFactory.CreateAsync(action, formStateData);

        string script;
        switch (action)
        {
            case InsertAction formAction:
                script = GetFormActionScript(action, formValues, contextAction, formAction.ShowAsPopup);
                break;
            case EditAction editAction:
                script = GetFormActionScript(action, formValues, contextAction, editAction.ShowAsPopup);
                break;
            case ViewAction viewAction:
                script = GetFormActionScript(action, formValues, contextAction, viewAction.ShowAsPopup);
                break;
            case ViewAction or DeleteAction or DeleteSelectedRowsAction or ImportAction
                or LogAction:
                script = GetFormActionScript(action, formValues, contextAction);
                break;
            case UrlRedirectAction redirectAction:
                script = GetUrlRedirectScript(redirectAction, formValues, contextAction, fieldName);
                break;
            case InternalAction internalAction:
                script = GetInternalUrlScript(internalAction, formValues);
                break;
            case ScriptAction jsAction:
                script = Expression.ParseExpression(jsAction.OnClientClick, pageState, false, formValues);
                break;
            case ConfigAction:
                script = BootstrapHelper.GetModalScript($"config_modal_{ComponentName}");
                break;
            case ExportAction:
                script = $"JJDataExp.openExportUI('{ComponentName}');";
                break;
            case SaveAction save:
                if (save.EnterKeyBehavior == FormEnterKey.Submit)
                    link.Type = LinkButtonType.Submit;
                else
                    link.Type = save.IsGroup ? LinkButtonType.Link : LinkButtonType.Button;

                script = $"return FormViewScripts.executePanelAction('{ComponentName}','OK');";
                break;
            case CancelAction or BackAction:
                script = $"return FormViewScripts.executePanelAction('{ComponentName}','CANCEL');";
                break;
            case RefreshAction:
                script = $"FormView.refresh('{ComponentName}');";
                break;
            case FilterAction filterAction:
                {
                    if (filterAction.ShowAsCollapse)
                        link.Visible = false;

                    script = BootstrapHelper.GetModalScript($"filter_modal_{ComponentName}");
                    break;
                }
            case LegendAction:
                script = BootstrapHelper.GetModalScript($"iconlegend_modal_{ComponentName}");
                break;
            case SqlCommandAction:
                script = GetCommandScript(action, formValues, contextAction);
                break;
            case SortAction:
                script = BootstrapHelper.GetModalScript($"sort_modal_{ComponentName}");
                break;
            case SubmitAction submitAction:
                link.UrlAction = submitAction.FormAction;
                string confirmationMessage = submitAction.ConfirmationMessage;
                if (!string.IsNullOrWhiteSpace(confirmationMessage))
                    script = $"return confirm('{confirmationMessage}');";
                else
                    script = string.Empty;

                break;
            default:
                throw new NotImplementedException();
        }

        link.OnClientClick = script;

        return link;
    }
}