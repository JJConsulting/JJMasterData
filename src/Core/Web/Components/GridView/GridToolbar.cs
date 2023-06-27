using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.DataDictionary.Actions.GridToolbar;
using JJMasterData.Core.DataDictionary.Actions.UserCreated;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Html;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace JJMasterData.Core.Web.Components;

internal class GridToolbar
{
    private JJGridView GridView { get; set; }

    public GridToolbar(JJGridView gridView)
    {
        GridView = gridView;
    }

    public HtmlBuilder GetHtmlElement()
    {
        var toolbar = new JJToolbar();
        toolbar.Items.AddRange(GetActionsHtmlElement());
        return toolbar.GetHtmlBuilder();
    }

    private IEnumerable<HtmlBuilder> GetActionsHtmlElement()
    {
        var actions = GridView.ToolBarActions.OrderBy(x => x.Order).ToList();
        var expressionManager = GridView.FieldManager.ExpressionManager;
        var formValues = GridView.DefaultValues;

        foreach (var action in actions)
        {
            bool isVisible = expressionManager.GetBoolValue(action.VisibleExpression, action.Name, PageState.List, formValues);
            if (!isVisible)
                continue;

            if (action is FilterAction { EnableScreenSearch: true })
            {
                yield return GridView.Filter.GetHtmlToolBarSearch();
                continue;
            }

            bool isEnabled = expressionManager.GetBoolValue(action.EnableExpression, action.Name, PageState.List, formValues);
            var linkButton = JJLinkButton.GetInstance(action, isEnabled, true);
            linkButton.OnClientClick = GetScriptAction(action, formValues);
            switch (action)
            {
                case ExportAction when GridView.DataExp.IsRunning():
                    linkButton.Spinner.Name = "dataexp_spinner_" + GridView.Name;
                    linkButton.Spinner.Visible = true;
                    break;
                case ImportAction when GridView.DataImp.IsRunning():
                    linkButton.Spinner.Visible = true;
                    break;
                case FilterAction fAction:
                    if (fAction.ShowAsCollapse)
                        linkButton.Visible = false;
                    break;
            }

            yield return linkButton.GetHtmlBuilder();

        }
    }

    private string GetScriptAction(BasicAction action, IDictionary formValues)
    {
        var contextAction = ActionSource.GridToolbar;
        var pageState = PageState.List;
        var expressionManager = GridView.FieldManager.ExpressionManager;
        string script;

        switch (action)
        {
            case UrlRedirectAction redirectAction:
                script = GridView.ActionManager.GetUrlRedirectScript(redirectAction, formValues, pageState, contextAction, null);
                break;
            case InternalAction internalAction:
                script = GridView.ActionManager.GetInternalUrlScript(internalAction, formValues);
                break;
            case SqlCommandAction:
                script = GridView.ActionManager.GetCommandScript(action, formValues, contextAction);
                break;
            case ScriptAction jsAction:
                script = expressionManager.ParseExpression(jsAction.OnClientClick, pageState, false, formValues);
                break;
            case InsertAction insertAction:
                script = GridView.ActionManager.GetFormActionScript(action, formValues, contextAction, insertAction.ShowAsPopup);
                break;
                case DeleteSelectedRowsAction or ImportAction or LogAction:
                script = GridView.ActionManager.GetFormActionScript(action, formValues, contextAction);
                break;
            case ConfigAction:
                script = BootstrapHelper.GetModalScript($"config_modal_{GridView.Name}");
                break;
            case ExportAction:
                script = $"JJDataExp.openExportUI('{GridView.Name}');";
                break;
            case RefreshAction:
                script = GridView.GetRefreshScript();
                break;
            case FilterAction:
                script = BootstrapHelper.GetModalScript($"filter_modal_{GridView.Name}");
                break;
            case LegendAction:
                script = BootstrapHelper.GetModalScript($"iconlegend_modal_{GridView.Name}");
                break;
            case SortAction:
                script = BootstrapHelper.GetModalScript($"sort_modal_{GridView.Name}");
                break;
            case SubmitAction submitAction:
                string confirmationMessage = submitAction.ConfirmationMessage;
                if (!string.IsNullOrWhiteSpace(confirmationMessage))
                    script = $"return confirm('{confirmationMessage}');";
                else
                    script = string.Empty;

                break;
            default:
                throw new NotImplementedException();
        }

        return script;
    }
}