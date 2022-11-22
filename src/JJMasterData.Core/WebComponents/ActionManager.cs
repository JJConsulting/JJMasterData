using JJMasterData.Commons.Dao;
using JJMasterData.Commons.Exceptions;
using JJMasterData.Commons.Language;
using JJMasterData.Commons.Util;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.DataDictionary.Repository;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Text;

namespace JJMasterData.Core.WebComponents;
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


    private string GetInternalUrlScript(InternalAction action, Hashtable formValues)
    {
        var elementRedirect = action.ElementRedirect;
        var dicDao = new DictionaryDao(EntityRepository);
        var dicParser = dicDao.GetDictionary(action.ElementRedirect.ElementNameRedirect);
        string popUpTitle = dicParser.Form.Title;
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
            if (formValues.ContainsKey(r.InternalField))
            {
                @params.Append("&");
                @params.Append(r.RedirectField);
                @params.Append("=");
                @params.Append(formValues[r.InternalField]);
            }
        }

        string url =
            $"{ConfigurationHelper.GetUrlMasterData()}InternalRedirect?parameters={Cript.EnigmaEncryptRP(@params.ToString())}";

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

    private string GetUrlRedirectScript(UrlRedirectAction action, Hashtable formValues, PageState pageState, ActionOrigin contextAction, string fieldName)
    {
        var actionMap = new ActionMap(contextAction, FormElement, formValues, action.Name);
        actionMap.FieldName = fieldName;
        string criptMap = actionMap.GetCriptJson();
        string confirmationMessage = Translate.Key(action.ConfirmationMessage);

        var script = new StringBuilder();

        if (contextAction == ActionOrigin.Field ||
            contextAction == ActionOrigin.Form)
        {
            script.Append("jjview.doFormUrlRedirect('");
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
            script.Append("');");
        }
        return script.ToString();
    }

    public string GetFormActionScript(BasicAction action, Hashtable formValues, ActionOrigin contextAction)
    {
        var actionMap = new ActionMap(contextAction, FormElement, formValues, action.Name);
        string criptMap = actionMap.GetCriptJson();
        string confirmationMessage = Translate.Key(action.ConfirmationMessage);

        var script = new StringBuilder();
        script.Append("jjview.formAction('");
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


    internal string GetExportScript(ExportAction action, Hashtable formValues)
    {
        var actionMap = new ActionMap(ActionOrigin.Toolbar, FormElement, formValues, action.Name);
        string criptMap = actionMap.GetCriptJson();

        var script = new StringBuilder();
        script.Append("JJDataExp.doExport('");
        script.Append(ComponentName);
        script.Append("','");
        script.Append(criptMap);
        script.Append("');");

        return script.ToString();
    }

    internal string GetConfigUIScript(ConfigAction action, Hashtable formValues)
    {
        var actionMap = new ActionMap(ActionOrigin.Toolbar, FormElement, formValues, action.Name);
        string criptMap = actionMap.GetCriptJson();

        var script = new StringBuilder();
        script.Append("jjview.doConfigUI('");
        script.Append(ComponentName);
        script.Append("','");
        script.Append(criptMap);
        script.Append("');");

        return script.ToString();
    }

    private string GetCommandScript(BasicAction action, Hashtable formValues, ActionOrigin contextAction)
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

    
    public JJLinkButton GetLinkGrid(BasicAction action, Hashtable formValues)
    {
        return GetLink(action, formValues, PageState.List, ActionOrigin.Grid);
    }

    public JJLinkButton GetLinkToolBar(BasicAction action, Hashtable formValues)
    {
        return GetLink(action, formValues, PageState.List, ActionOrigin.Toolbar);
    }

    public JJLinkButton GetLinkField(BasicAction action, Hashtable formValues, PageState pagestate, string panelName)
    {
        return GetLink(action, formValues, pagestate, ActionOrigin.Field, panelName);
    }

    private JJLinkButton GetLink(BasicAction action, Hashtable formValues, PageState pagestate, ActionOrigin contextAction, string fieldName = null)
    {
        var link = new JJLinkButton
        {
            ToolTip = action.ToolTip,
            Text = action.Text,
            IsGroup = action.IsGroup,
            IsDefaultOption = action.IsDefaultOption,
            DividerLine = action.DividerLine,
            ShowAsButton = action.ShowAsButton,
            CssClass = action.CssClass,
            IconClass = IconHelper.GetClassName(action.Icon) + " fa-fw",
            Enabled = Expression.GetBoolValue(action.EnableExpression, action.Name, pagestate, formValues),
            Visible = Expression.GetBoolValue(action.VisibleExpression, action.Name, pagestate, formValues)
        };

        string script;
        switch (action)
        {
            case ViewAction or InsertAction or EditAction or DeleteAction or DeleteSelectedRowsAction or ImportAction or LogAction:
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
            default:
            {
                if (action is SqlCommandAction | action is PythonScriptAction)
                {
                    script = GetCommandScript(action, formValues, contextAction);
                }
                else if (action is SortAction)
                {
                    script = BootstrapHelper.GetModalScript($"sort_modal_{ComponentName}");
                }
                else
                {
                    throw new NotImplementedException();
                }

                break;
            }
        }

        link.OnClientClick = script;

        return link;
    }

    public string ExecutePythonScriptAction(JJGridView gridView, ActionMap map, PythonScriptAction action)
    {

        var scriptManager = FormEventEngineFactory.GetEngine<IPythonEngine>();

        try
        {
            if (map.ContextAction == ActionOrigin.Toolbar && gridView.EnableMultSelect && action.ApplyOnSelected)
            {
                var selectedRows = gridView.GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = Translate.Key("No lines selected.");
                    return new JJMessageBox(msg, MessageIcon.Warning).GetHtml();
                }

                foreach (var row in selectedRows)
                    scriptManager.Execute(Expression.ParseExpression(action.PythonScript, PageState.List, false, row));

                gridView.ClearSelectedGridValues();
            }
            else
            {
                Hashtable formValues;
                if (map.PKFieldValues != null && (map.PKFieldValues != null ||
                                                  map.PKFieldValues.Count > 0))
                {
                    formValues = gridView.EntityRepository.GetFields(FormElement, map.PKFieldValues);
                }
                else
                {
                    var formManager = new FormManager(FormElement, Expression);
                    formValues = formManager.GetDefaultValues(null, PageState.List);
                }
                scriptManager.Execute(Expression.ParseExpression(action.PythonScript, PageState.List, false, formValues));
            }
        }
        catch (Exception ex)
        {
            string msg = ExceptionManager.GetMessage(ex);
            return new JJMessageBox(msg, MessageIcon.Error).GetHtml();
        }

        return null;
    }

    public string ExecuteSqlCommand(JJGridView gridView, ActionMap map, SqlCommandAction cmdAction)
    {
        try
        {
            var listSql = new ArrayList();
            if (map.ContextAction == ActionOrigin.Toolbar && gridView.EnableMultSelect && cmdAction.ApplyOnSelected)
            {
                var selectedRows = gridView.GetSelectedGridValues();
                if (selectedRows.Count == 0)
                {
                    string msg = Translate.Key("No lines selected.");
                    return new JJMessageBox(msg, MessageIcon.Warning).GetHtml();
                }

                foreach (var row in selectedRows)
                {
                    string sql = this.Expression.ParseExpression(cmdAction.CommandSQL, PageState.List, false, row);
                    listSql.Add(sql);
                }

                EntityRepository.SetCommand(listSql);
                gridView.ClearSelectedGridValues();
            }
            else
            {
                Hashtable formValues;
                if (map.PKFieldValues != null && (map.PKFieldValues != null ||
                                                  map.PKFieldValues.Count > 0))
                {
                    formValues = gridView.EntityRepository.GetFields(FormElement, map.PKFieldValues);
                }
                else
                {
                    var formManager = new FormManager(FormElement, Expression);
                    formValues = formManager.GetDefaultValues(null, PageState.List);
                }

                string sql = Expression.ParseExpression(cmdAction.CommandSQL, PageState.List, false, formValues);
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
