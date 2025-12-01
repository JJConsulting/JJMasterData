using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Tasks;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Events.Args;


namespace JJMasterData.Core.UI.Components;

internal sealed class GridToolbar(JJGridView gridView)
{
    internal event AsyncEventHandler<GridToolbarActionEventArgs> OnRenderToolbarActionAsync;
    
    public async ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var toolbar = new JJToolbar();

        await AddActionsToToolbar(toolbar);
            
        return toolbar.GetHtmlBuilder().WithCssClass("mb-1");
    }

    private async ValueTask AddActionsToToolbar(JJToolbar toolbar)
    {
        var actions = gridView
            .ToolbarActions
            .OrderBy(a => a.Order);
        
        var actionButtonFactory = gridView.ComponentFactory.ActionButton;
        var formStateData = await gridView.GetFormStateDataAsync();

        var groupedActions = new List<JJLinkButton>();
        
        foreach (var action in actions)
        {
            var linkButton = actionButtonFactory.CreateGridToolbarButton(action, gridView,formStateData);
            if (!linkButton.Visible)
                continue;

            switch (action)
            {
                case InsertAction { ShowOpenedAtGrid: true }:
                    continue;
                case FilterAction { EnableScreenSearch: true }:
                    toolbar.Items.Add(gridView.Filter.GetHtmlToolBarSearch());
                    continue;
            }

            switch (action)
            {
                case ExportAction when gridView.DataExportation.IsRunning():
                    linkButton.Spinner.Name = $"data-exportation-spinner-{gridView.DataExportation.Name}";
                    linkButton.Spinner.Visible = true;
                    break;
                case ImportAction when gridView.DataImportation.IsRunning():
                    linkButton.Spinner.Visible = true;
                    break;
                case FilterAction fAction:
                    if (fAction.ShowAsCollapse)
                        linkButton.Visible = false;
                    break;
            }

            if (OnRenderToolbarActionAsync is not null)
            {
                var args = new GridToolbarActionEventArgs(action, linkButton);
                await OnRenderToolbarActionAsync(gridView, args);

                if (args.HtmlResult is not null)
                {
                    toolbar.Items.Add(new HtmlBuilder(args.HtmlResult));
                    continue;
                }
            }

            if(!action.IsGroup)
                toolbar.Items.Add(linkButton.GetHtmlBuilder());
            else
                groupedActions.Add(linkButton);
        }

        if (groupedActions.Count > 0)
        {
            var groupedAction = new JJLinkButtonGroup();
            groupedAction.ShowAsButton = true;
            groupedAction.CaretText = gridView.StringLocalizer["More"];
            groupedAction.CssClass = BootstrapHelper.PullRight;
            groupedAction.Actions = groupedActions;
            
            toolbar.Items.Add(groupedAction.GetHtmlBuilder());
        }
    }
}