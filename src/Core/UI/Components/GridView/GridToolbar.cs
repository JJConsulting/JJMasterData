using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

internal class GridToolbar(JJGridView gridView)
{
    private JJGridView GridView { get; } = gridView;

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var toolbar = new JJToolbar();
        
        await foreach(var action in GetActionsHtmlBuilderEnumerable())
        {
            toolbar.Items.Add(action);
        }
        
        return toolbar.GetHtmlBuilder().WithCssClass("mb-1");
    }

    private async IAsyncEnumerable<HtmlBuilder> GetActionsHtmlBuilderEnumerable()
    {
        var actions = GridView.ToolBarActions.OrderBy(x => x.Order).ToList();
        var actionButtonFactory = GridView.ComponentFactory.ActionButton;
        var formStateData = await GridView.GetFormStateDataAsync();
        
        foreach (var action in actions)
        {
            var linkButton = actionButtonFactory.CreateGridToolbarButton(action, GridView,formStateData);
            if (!linkButton.Visible)
                continue;

            if (action is FilterAction { EnableScreenSearch: true })
            {
                yield return await GridView.Filter.GetHtmlToolBarSearch();
                continue;
            }
            
            switch (action)
            {
                case ExportAction when GridView.DataExportation.IsRunning():
                    linkButton.Spinner.Name = $"data-exportation-spinner-{GridView.DataExportation.Name}";
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
    
}