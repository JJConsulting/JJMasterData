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
using System.Threading.Tasks;


namespace JJMasterData.Core.Web.Components;

internal class GridToolbar
{
    private JJGridView GridView { get; set; }

    public GridToolbar(JJGridView gridView)
    {
        GridView = gridView;
    }

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var toolbar = new JJToolbar();
        
        await foreach(var action in GetActionsHtmlElement())
        {
            toolbar.Items.Add(action);
        }
        
        return toolbar.GetHtmlBuilder();
    }

    private async IAsyncEnumerable<HtmlBuilder> GetActionsHtmlElement()
    {
        var actions = GridView.ToolBarActions.OrderBy(x => x.Order).ToList();
        var expressionsService = GridView.ExpressionsService;
        var formValues = await GridView.GetDefaultValuesAsync();

        foreach (var action in actions)
        {
            bool isVisible = await expressionsService.GetBoolValueAsync	(action.VisibleExpression, PageState.List, formValues);
            if (!isVisible)
                continue;

            if (action is FilterAction { EnableScreenSearch: true })
            {
                yield return GridView.Filter.GetHtmlToolBarSearch();
                continue;
            }

            bool isEnabled = await expressionsService.GetBoolValueAsync(action.EnableExpression, PageState.List, formValues);
            var linkButton = JJLinkButton.GetInstance(action, isEnabled, true);
            linkButton.OnClientClick = GetScriptAction(action, formValues);
            switch (action)
            {
                case ExportAction when GridView.DataExportation.IsRunning():
                    linkButton.Spinner.Name = "dataexp_spinner_" + GridView.Name;
                    linkButton.Spinner.Visible = true;
                    break;
                case ImportAction when GridView.DataImportation.IsRunning():
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

    private string GetScriptAction(BasicAction action, IDictionary<string,dynamic>formValues)
    {
        var contextAction = ActionSource.GridToolbar;
        var pageState = PageState.List;
        var expressionsService = GridView.ExpressionsService;
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
                script = expressionsService.ParseExpression(jsAction.OnClientClick, pageState, false, formValues);
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
                script = GridView.DataExportation.Scripts.GetExportPopupScript(
                    GridView.FormElement.Name,
                        GridView.Name,
                    GridView.IsExternalRoute);
                break;
            case RefreshAction:
                script = GridView.Scripts.GetRefreshScript(GridView);
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
                script = !string.IsNullOrWhiteSpace(confirmationMessage) ? $"return confirm('{confirmationMessage}');" : string.Empty;

                break;
            default:
                throw new NotImplementedException();
        }

        return script;
    }
}