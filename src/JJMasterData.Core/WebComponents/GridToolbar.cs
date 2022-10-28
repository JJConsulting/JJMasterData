using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Action;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

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
        toolbar.ListElement.AddRange(GetActionsHtmlElement());
        return toolbar.GetHtmlBuilder();
    }

    private IList<HtmlBuilder> GetActionsHtmlElement()
    {
        var htmlList = new List<HtmlBuilder>();
        var actions = GridView.ToolBarActions.OrderBy(x => x.Order).ToList();
        
        foreach (var action in actions)
        {
            if (action is FilterAction filterAction)
            {
                bool isVisible = GridView.FieldManager.IsVisible(action, PageState.List, GridView.DefaultValues);
                if (!isVisible)
                    continue;

                if (filterAction.EnableScreenSearch)
                {
                    htmlList.Add(GridView.Filter.GetHtmlToolBarSearch());
                    continue;
                }
            }

            var linkButton = GridView.ActionManager.GetLinkToolBar(action, GridView.DefaultValues);
            if (linkButton.Visible)
            {
                if (action is ExportAction && GridView.DataExp.IsRunning())
                {
                    linkButton.Spinner.Name = "dataexp_spinner_" + GridView.Name;
                    linkButton.Spinner.Visible = true;
                }
                else if (action is ImportAction && GridView.DataImp.IsRunning())
                {
                    linkButton.Spinner.Visible = true;
                }

                htmlList.Add(linkButton.GetHtmlBuilder());
            }
        }

        return htmlList;
    }
}