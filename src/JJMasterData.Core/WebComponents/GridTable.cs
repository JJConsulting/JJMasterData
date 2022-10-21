using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

internal class GridTable
{
    private JJGridView GridView { get; }

    public GridTable(JJGridView gridView)
    {
        GridView = gridView;
    }

    public HtmlElement GetHtmlElement()
    {
        var div = new HtmlElement(HtmlTag.Div)
            .WithCssClassIf(GridView.CurrentUI.IsResponsive, "table-responsive")
            .AppendElement(HtmlTag.Table, table =>
            {
                var ui = GridView.CurrentUI;

                table.WithCssClass("table")
                    .WithCssClassIf(ui.IsResponsive, "table-responsive")
                    .WithCssClassIf(ui.ShowBorder, "table-bordered")
                    .WithCssClassIf(ui.IsResponsive, "table-hover")
                    .WithCssClassIf(ui.ShowRowStriped, "table-striped")
                    .WithCssClassIf(ui.IsHeaderFixed, "table-fix-head")
                    .AppendElement(new GridTableHeader(GridView).GetHtmlElement());
            });

        return div;
    }
}