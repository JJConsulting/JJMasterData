using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JJMasterData.Commons.Configuration;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity.Abstractions;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.FormToolbar;
using JJMasterData.Core.DataDictionary.Actions.GridTable;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.DataDictionary.Repository.Abstractions;
using JJMasterData.Core.DataManager.Services.Abstractions;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager;

//TODO: Remove this class
//TODO: When removing this class DON'T use JJService, see DataManager/Services/
internal class ActionManager
{
    /// <summary>
    /// <see cref="FormElement"/>
    /// </summary>
    public FormElement FormElement { get; private set; }

    public IExpressionsService Expression { get; private set; }

    public string ComponentName { get; set; }

    internal IEntityRepository EntityRepository => JJService.Provider.GetRequiredService<IEntityRepository>();

    internal IStringLocalizer<JJMasterDataResources> StringLocalizer =>
        JJService.Provider.GetRequiredService<IStringLocalizer<JJMasterDataResources>>();

    internal IFieldValuesService FieldValuesService =>
        JJService.Provider.GetScopedDependentService<IFieldValuesService>();

    internal JJMasterDataEncryptionService JJMasterDataEncryptionService =>
        JJService.Provider.GetScopedDependentService<JJMasterDataEncryptionService>();

    public ActionManager(FormElement formElement, IExpressionsService expression, string panelName)
    {
        FormElement = formElement;
        Expression = expression;
        ComponentName = panelName;
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
                parameters = JJMasterDataEncryptionService.EncryptStringWithUrlEscape(@params.ToString()),
                Area = "MasterData"
            });

        return $"JJView.executeRedirectAction('{url}',{popup},'{popUpTitle}','{confirmationMessage}','{popupSize}');";
    }


    public string GetUrlRedirectScript(UrlRedirectAction action, IDictionary<string, dynamic> formValues,
        PageState pageState,
        ActionSource contextAction, string fieldName)
    {
        var actionMap = new ActionMap(contextAction, FormElement, formValues, action.Name)
        {
            FieldName = fieldName
        };
        var encryptedActionMap = JJMasterDataEncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage ?? string.Empty];
        int popupSize = (int)action.PopupSize;

        if (contextAction is ActionSource.Field or ActionSource.FormToolbar)
        {
            return
                $"JJView.executeRedirectAction('{ComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }

        string url = Expression.ParseExpression(action.UrlRedirect, pageState, false, formValues);
        string popup = action.UrlAsPopUp ? "true" : "false";
        string popUpTitle = action.PopUpTitle;

        return $"JJView.executeRedirectAction('{url}',{popup},'{popUpTitle}','{confirmationMessage}','{popupSize}');";
    }


    public string GetFormActionScript(BasicAction action, IDictionary<string, dynamic> formValues,
        ActionSource actionSource, bool isPopup = false)
    {
        var actionMap = new ActionMap(actionSource, FormElement, formValues, action.Name);
        var encryptedActionMap = JJMasterDataEncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage ?? string.Empty];

        string functionSignature;
        if (isPopup)
        {
            var url = GetFormViewUrl(FormElement.Name, action, actionMap);
            functionSignature =
                $"ActionManager.executeFormActionAsPopUp('{url}','{ComponentName}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
        }
        else
        {
            functionSignature =
                $"ActionManager.executeFormAction('{ComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
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

    internal string GetExportScript(ExportAction action, IDictionary<string, dynamic> formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, FormElement, formValues, action.Name);
        var encryptedActionMap = JJMasterDataEncryptionService.EncryptActionMap(actionMap);

        return $"JJDataExp.doExport('{ComponentName}','{encryptedActionMap}');";
    }


    internal string GetConfigUIScript(ConfigAction action, IDictionary<string, dynamic> formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, FormElement, formValues, action.Name);
        string encryptedActionMap = JJMasterDataEncryptionService.EncryptActionMap(actionMap);

        return $"JJView.openSettingsModal('{ComponentName}','{encryptedActionMap}');";
    }

    public string GetCommandScript(BasicAction action, IDictionary<string, dynamic> formValues,
        ActionSource contextAction)
    {
        var actionMap = new ActionMap(contextAction, FormElement, formValues, action.Name);
        string encryptedActionMap = JJMasterDataEncryptionService.EncryptActionMap(actionMap);
        string confirmationMessage = StringLocalizer[action.ConfirmationMessage];

        return
            $"JJView.executeGridAction('{ComponentName}','{encryptedActionMap}'{(string.IsNullOrEmpty(confirmationMessage) ? "" : $",'{confirmationMessage}'")});";
    }


    public JJLinkButton GetLinkGrid(BasicAction action, IDictionary<string, dynamic> formValues)
    {
        return GetLink(action, formValues, PageState.List, ActionSource.GridTable);
    }

    //public JJLinkButton GetLinkGridToolbar(BasicAction action, IDictionary<string,dynamic>formValues)
    //{
    //    return GetLink(action, formValues, PageState.List, ActionSource.GridToolbar);
    //}

    public JJLinkButton GetLinkFormToolbar(BasicAction action, IDictionary<string, dynamic> formValues,
        PageState pageState)
    {
        return GetLink(action, formValues, pageState, ActionSource.FormToolbar);
    }

    public JJLinkButton GetLinkField(BasicAction action, IDictionary<string, dynamic> formValues, PageState pageState,
        string panelName)
    {
        return GetLink(action, formValues, pageState, ActionSource.Field, panelName);
    }

    private static HtmlBuilder GetDividerHtml()
    {
        var li = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("separator")
            .WithCssClass("divider");

        return li;
    }


    private JJLinkButton GetLink(BasicAction action, IDictionary<string, dynamic> formValues, PageState pagestate,
        ActionSource contextAction, string fieldName = null)
    {
        var enabled = Expression.GetBoolValueAsync(action.EnableExpression, pagestate, formValues).GetAwaiter()
            .GetResult();
        var visible = Expression.GetBoolValueAsync(action.VisibleExpression, pagestate, formValues).GetAwaiter()
            .GetResult();
        var link = JJLinkButton.GetInstance(action, enabled, visible);

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
                script = GetUrlRedirectScript(redirectAction, formValues, pagestate, contextAction, fieldName);
                break;
            case InternalAction internalAction:
                script = GetInternalUrlScript(internalAction, formValues);
                break;
            case ScriptAction jsAction:
                script = Expression.ParseExpression(jsAction.OnClientClick, pagestate, false, formValues);
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

                script = $"return ActionManager.executePanelAction('{ComponentName}','OK');";
                break;
            case CancelAction or BackAction:
                script = $"return ActionManager.executePanelAction('{ComponentName}','CANCEL');";
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