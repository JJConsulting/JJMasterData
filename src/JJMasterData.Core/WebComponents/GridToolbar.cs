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

    public HtmlElement GetHtmlElement()
    {
        var toolbar = new HtmlElement(HtmlTag.Div)
            .WithCssClass(BootstrapHelper.FormGroup)
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass("row");
                div.AppendElement(HtmlTag.Div, div =>
                {
                    div.WithCssClass("col-sm-12");
                    var listActions = GridView.ToolBarActions.OrderBy(x => x.Order).ToList();
                    foreach (var action in listActions)
                    {
                        if (action is FilterAction filterAction)
                        {
                            bool isVisible =
                                GridView.FieldManager.IsVisible(action, PageState.List, GridView.DefaultValues);
                            if (!isVisible)
                                continue;

                            if (filterAction.EnableScreenSearch)
                            {
                                div.AppendText(GridView.Filter.GetHtmlToolBarSeach());
                                continue;
                            }
                        }

                        var linkButton = GridView.ActionManager.GetLinkToolBar(action, GridView.DefaultValues);
                        if (linkButton.Visible)
                        {
                            switch (action)
                            {
                                case ExportAction:
                                {
                                    if (GridView.DataExp.IsRunning())
                                    {
                                        linkButton.Spinner.Name = "dataexp_spinner_" + GridView.Name;
                                        linkButton.Spinner.Visible = true;
                                    }

                                    break;
                                }
                                case ImportAction:
                                {
                                    if (GridView.DataImp.IsRunning())
                                        linkButton.Spinner.Visible = true;
                                    break;
                                }
                            }
                        }


                        if (BootstrapHelper.Version != 3)
                        {
                            linkButton.CssClass += $" {BootstrapHelper.MarginRight}-1";
                        }

                        div.AppendElement(linkButton);
                    }
                });
            });

        return toolbar;
    }
}