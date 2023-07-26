using System.Data;
using System.Threading.Tasks;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

internal class GridTable
{
    internal GridSettings Settings { get; }

    internal GridTableHeader Header { get; }

    internal GridTableBody Body { get; }

    internal DataTable DataSource { get; }

    internal string EmptyDataText { get; }

    internal GridFilter Filter { get; }

    public GridTable(JJGridView gridView)
    {
        Header = new GridTableHeader(gridView);
        Body = new GridTableBody(gridView);
        Settings = gridView.CurrentSettings;
        DataSource = gridView.DataSource;
        EmptyDataText = gridView.EmptyDataText;
        Filter = gridView.Filter;
    }

    public async Task<HtmlBuilder> GetHtmlElement()
    {
        var table = new HtmlBuilder(HtmlTag.Table);

        table.WithCssClass("table");

        table.WithCssClassIf(Settings.IsResponsive, "table-responsive");
        table.WithCssClassIf(Settings.ShowBorder, "table-bordered");
        table.WithCssClassIf(Settings.IsResponsive, "table-hover");
        table.WithCssClassIf(Settings.ShowRowStriped, "table-striped");
        table.WithCssClassIf(Settings.IsHeaderFixed, "table-fix-head");

        table.AppendElement(Header.GetHtmlElement());
        table.AppendElement(await Body.GetHtmlElementAsync());

        return table;
    }
}