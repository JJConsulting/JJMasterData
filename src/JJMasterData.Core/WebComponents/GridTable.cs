using System;
using System.Data;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

internal class GridTable
{
    internal GridUI Ui { get; }

    internal GridTableHeader Header { get; }

    internal GridTableBody Body { get; }

    internal DataTable DataSource { get; }

    internal string EmptyDataText { get; }

    internal GridFilter Filter { get; }

    public GridTable(JJGridView gridView)
    {
        Header = new GridTableHeader(gridView);
        Body = new GridTableBody(gridView);
        Ui = gridView.CurrentUI;
        DataSource = gridView.DataSource;
        EmptyDataText = gridView.EmptyDataText;
        Filter = gridView.Filter;
    }

    public HtmlElement GetHtmlElement()
    {
        var table = new HtmlElement(HtmlTag.Table);

        table.WithCssClass("table");

        table.WithCssClassIf(Ui.IsResponsive, "table-responsive");
        table.WithCssClassIf(Ui.ShowBorder, "table-bordered");
        table.WithCssClassIf(Ui.IsResponsive, "table-hover");
        table.WithCssClassIf(Ui.ShowRowStriped, "table-striped");
        table.WithCssClassIf(Ui.IsHeaderFixed, "table-fix-head");

        table.AppendElement(Header.GetHtmlElement());
        table.AppendElement(Body.GetHtmlElement());

        return table;
    }
}