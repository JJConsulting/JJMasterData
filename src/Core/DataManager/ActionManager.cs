using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
using JJMasterData.Core.DI;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.FormEvents.Args;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager;

//TODO: Remove this class
internal class ActionManager
{
    /// <summary>
    /// <see cref="FormElement"/>
    /// </summary>
    public FormElement FormElement { get; private set; }

    public ExpressionManager Expression { get; private set; }

    public string ComponentName { get; set; }

    internal IEntityRepository EntityRepository => Expression.EntityRepository;


    public ActionManager(FormElement formElement, ExpressionManager expression, string panelName)
    {
        FormElement = formElement;
        Expression = expression;
        ComponentName = panelName;
    }


    public string GetInternalUrlScript(InternalAction action, IDictionary formValues)
    {
        var elementRedirect = action.ElementRedirect;
        var dicRepository = JJServiceCore.DataDictionaryRepository;
        var formElement = dicRepository.GetMetadata(action.ElementRedirect.ElementNameRedirect);
        string popUpTitle = formElement.Title;
        string confirmationMessage = Translate.Key(action.ConfirmationMessage);
        string popup = "true";
        int popupSize = (int)elementRedirect.PopupSize;

        var @params = new StringBuilder();

        @params.Append("formname=");
        @params.Append(elementRedirect.ElementNameRedirect);
        @params.Append("&viewtype=");
        @params.Append((int)elementRedirect.ViewType);

        foreach (var r in elementRedirect.RelationFields)
        {
            if (formValues.Contains(r.InternalField))
            {
                @params.Append("&");
                @params.Append(r.RedirectField);
                @params.Append("=");
                @params.Append(formValues[r.InternalField]);
            }
        }

        
        var urlHelper = JJMasterDataUrlHelper.GetInstance();
        string url = urlHelper.GetUrl(null,"InternalRedirect", new { parameters =  Cript.EnigmaEncryptRP(@params.ToString()), Area="MasterData"});

        var script = new StringBuilder();
        script.Append("jjview.doUrlRedirect('");
        script.Append(url);
        script.Append("',");
        script.Append(popup);
        script.Append(",'");
        script.Append(popUpTitle);
        script.Append("','");
        script.Append(confirmationMessage);
        script.Append("','");
        script.Append(popupSize);
        script.Append("');");

        return script.ToString();
    }

    public string GetUrlRedirectScript(UrlRedirectAction action, IDictionary formValues, PageState pageState,
        ActionSource contextAction, string fieldName)
    {
        var actionMap = new ActionMap(contextAction, FormElement, formValues, action.Name);
        actionMap.FieldName = fieldName;
        string criptMap = actionMap.GetCriptJson();
        string confirmationMessage = Translate.Key(action.ConfirmationMessage);
        int popupSize = (int)action.PopupSize;
        
        var script = new StringBuilder();

        if (contextAction is ActionSource.Field or ActionSource.FormToolbar)
        {
            script.Append("jjview.doFormUrlRedirect('");
            script.Append(ComponentName);
            script.Append("','");
            script.Append(criptMap);
            script.Append('\'');
            if (!string.IsNullOrEmpty(confirmationMessage))
            {
                script.Append(",'");
                script.Append(confirmationMessage);
                script.Append('\'');
            }

            script.Append(");");
        }
        else
        {
            string url = Expression.ParseExpression(action.UrlRedirect, pageState, false, formValues);
            string popup = action.UrlAsPopUp ? "true" : "false";
            string popUpTitle = action.TitlePopUp;

            script.Append("jjview.doUrlRedirect('");
            script.Append(url);
            script.Append("',");
            script.Append(popup);
            script.Append(",'");
            script.Append(popUpTitle);
            script.Append("','");
            script.Append(confirmationMessage);
            script.Append("','");
            script.Append(popupSize);
            script.Append("');");
        }

        return script.ToString();
    }

    public string GetFormActionScript(BasicAction action, IDictionary formValues, ActionSource actionSource, bool isPopup = false)
    {
        var actionMap = new ActionMap(actionSource, FormElement, formValues, action.Name);
        string encryptedActionMap = actionMap.GetCriptJson();
        string confirmationMessage = Translate.Key(action.ConfirmationMessage);

        string functionSignature;
        if (isPopup)
        {
            var url = GetFormViewUrl(FormElement.Name, action, actionMap);
            functionSignature = $"ActionManager.executeFormActionAsPopUp('{url}','";
            var script = new StringBuilder();
            script.Append(functionSignature);
            script.Append(ComponentName);
            script.Append('\'');
            if (!string.IsNullOrEmpty(confirmationMessage))
            {
                script.Append(",'");
                script.Append(confirmationMessage);
                script.Append('\'');
            }

            script.Append(");");

            return script.ToString();
        }
        else
        {
            functionSignature = "ActionManager.executeFormAction('";
            var script = new StringBuilder();
            script.Append(functionSignature);
            script.Append(ComponentName);
            script.Append("','");
            script.Append(encryptedActionMap);
            script.Append('\'');
            if (!string.IsNullOrEmpty(confirmationMessage))
            {
                script.Append(",'");
                script.Append(confirmationMessage);
                script.Append('\'');
            }

            script.Append(");");

            return script.ToString();
        }


    }
    private static string GetFormViewUrl(string dictionaryName, BasicAction action, ActionMap actionMap)
    {
        var encryptionService = JJService.Provider.GetService<JJMasterDataEncryptionService>();
        string encryptedDictionaryName = encryptionService.EncryptStringWithUrlEncode(dictionaryName);


        var pageState = action switch
        {
            InsertAction => PageState.Insert,
            ViewAction => PageState.View,
            _ => PageState.Update
        };

        var urlHelper = JJMasterDataUrlHelper.GetInstance();
        var encryptedActionMap = encryptionService.EncryptActionMap(actionMap);
        return urlHelper.GetUrl("GetFormView", "Form", new { 
            dictionaryName = encryptedDictionaryName, 
            actionMap = encryptedActionMap, 
            pageState, 
            Area="MasterData"});
    }

    internal string GetExportScript(ExportAction action, Hashtable formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, FormElement, formValues, action.Name);
        string criptMap = actionMap.GetCriptJson();

        var script = new StringBuilder();
        script.Append("JJDataExp.doExport('");
        script.Append(ComponentName);
        script.Append("','");
        script.Append(criptMap);
        script.Append("');");

        return script.ToString();
    }

    internal string GetConfigUIScript(ConfigAction action, IDictionary formValues)
    {
        var actionMap = new ActionMap(ActionSource.GridToolbar, FormElement, formValues, action.Name);
        string criptMap = actionMap.GetCriptJson();

        var script = new StringBuilder();
        script.Append("jjview.doConfigUI('");
        script.Append(ComponentName);
        script.Append("','");
        script.Append(criptMap);
        script.Append("');");

        return script.ToString();
    }

    public string GetCommandScript(BasicAction action, IDictionary formValues, ActionSource contextAction)
    {
        var actionMap = new ActionMap(contextAction, FormElement, formValues, action.Name);
        string jsonMap = JsonConvert.SerializeObject(actionMap);
        string criptMap = Cript.Cript64(jsonMap);
        string confirmationMessage = Translate.Key(action.ConfirmationMessage);

        var script = new StringBuilder();
        script.Append("jjview.gridAction('");
        script.Append(ComponentName);
        script.Append("','");
        script.Append(criptMap);
        script.Append("'");
        if (!string.IsNullOrEmpty(confirmationMessage))
        {
            script.Append(",'");
            script.Append(confirmationMessage);
            script.Append("'");
        }

        script.Append(");");

        return script.ToString();
    }


    public JJLinkButton GetLinkGrid(BasicAction action, IDictionary formValues)
    {
        return GetLink(action, formValues, PageState.List, ActionSource.GridTable);
    }

    //public JJLinkButton GetLinkGridToolbar(BasicAction action, IDictionary formValues)
    //{
    //    return GetLink(action, formValues, PageState.List, ActionSource.GridToolbar);
    //}

    public JJLinkButton GetLinkFormToolbar(BasicAction action, IDictionary formValues, PageState pageState)
    {
        return GetLink(action, formValues, pageState, ActionSource.FormToolbar);
    }

    public JJLinkButton GetLinkField(BasicAction action, IDictionary formValues, PageState pageState, string panelName)
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

    public HtmlBuilder GetGroupedActionsHtml(List<BasicAction> actionsWithGroup, ActionContext actionContext)
    {
        var td = new HtmlBuilder(HtmlTag.Td)
            .WithCssClass("table-action")
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass(BootstrapHelper.InputGroupBtn);
                div.AppendElement(BootstrapHelper.Version == 3 ? HtmlTag.Button : HtmlTag.A,
                    element =>
                    {
                        element.WithAttribute("type", "button");
                        element.WithCssClassIf(actionContext.PageState is PageState.List, "btn-link");
                        element.WithCssClassIf(actionContext.PageState is not PageState.List, "btn btn-secondary");
                        element.WithCssClass("dropdown-toggle");
                        element.WithAttribute(BootstrapHelper.DataToggle, "dropdown");
                        element.WithAttribute("aria-haspopup", "true");
                        element.WithAttribute("aria-expanded", "false");
                        element.AppendTextIf(actionContext.PageState is not PageState.List, Translate.Key("More"));
                        element.AppendElement(HtmlTag.Span, span =>
                        {
                            span.WithCssClass("caret");
                            span.WithToolTip(Translate.Key("More Options"));
                        });
                    });
                div.AppendElement(HtmlTag.Ul, ul =>
                {
                    ul.WithCssClass("dropdown-menu dropdown-menu-right");
                    foreach (var action in actionsWithGroup)
                    {
                        var link = actionContext.Source == ActionSource.GridTable
                            ? GetLinkGrid(action, actionContext.Values)
                            : GetLinkFormToolbar(action, actionContext.Values, actionContext.PageState);

                        link.Attributes.Add("style", "display:block");

                        var onRender = actionContext.OnRenderAction;
                        if (onRender != null)
                        {
                            var args = new ActionEventArgs(action, link, actionContext.Values);
                            onRender.Invoke(this, args);
                        }

                        if (link is { Visible: true })
                        {
                            ul.AppendElementIf(action.DividerLine, GetDividerHtml);
                            ul.AppendElement(HtmlTag.Li, li =>
                            {
                                li.WithCssClass("dropdown-item");
                                li.AppendElement(link);
                            });
                        }
                    }
                });
            });
        return td;
    }


    private JJLinkButton GetLink(BasicAction action, IDictionary formValues, PageState pagestate,
        ActionSource contextAction, string fieldName = null)
    {
        var enabled = Expression.GetBoolValue(action.EnableExpression, action.Name, pagestate, formValues);
        var visible = Expression.GetBoolValue(action.VisibleExpression, action.Name, pagestate, formValues);
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
                script = $"jjview.doRefresh('{ComponentName}');";
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

    public string ExecuteSqlCommand(JJGridView gridView, ActionMap map, SqlCommandAction cmdAction)
    {
        try
        {
            var listSql = new List<string>();
            if (map.ActionSource == ActionSource.GridToolbar && gridView.EnableMultSelect && cmdAction.ApplyOnSelected)
            {
                var selectedRows = gridView.GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = Translate.Key("No lines selected.");
                    return new JJMessageBox(msg, MessageIcon.Warning).GetHtml();
                }

                foreach (var row in selectedRows)
                {
                    string sql = Expression.ParseExpression(cmdAction.CommandSql, PageState.List, false, row);
                    listSql.Add(sql);
                }

                EntityRepository.SetCommand(listSql);
                gridView.ClearSelectedGridValues();
            }
            else
            {
                Hashtable formValues;
                if (map.PkFieldValues != null && (map.PkFieldValues != null ||
                                                  map.PkFieldValues.Count > 0))
                {
                    formValues = gridView.EntityRepository.GetFields(FormElement, map.PkFieldValues);
                }
                else
                {
                    var formManager = new FormManager(FormElement, Expression);
                    formValues = formManager.GetDefaultValues(null, PageState.List);
                }

                string sql = Expression.ParseExpression(cmdAction.CommandSql, PageState.List, false, formValues);
                listSql.Add(sql);
                EntityRepository.SetCommand(listSql);
            }
        }
        catch (Exception ex)
        {
            string msg = ExceptionManager.GetMessage(ex);
            return new JJMessageBox(msg, MessageIcon.Error).GetHtml();
        }

        return null;
    }
}