#nullable enable
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;


namespace JJMasterData.Core.UI.Components;

internal sealed class GridTable(JJGridView gridView)
{
    internal GridSettings Settings { get; } = gridView.CurrentSettings;

    internal GridTableHeader Header { get; } = new(gridView);

    internal GridTableBody Body { get; } = new(gridView);

    public async ValueTask<HtmlBuilder> GetHtmlBuilder()
    {
        var div = HtmlBuilder.Div();
        div.WithCssClass("pt-1");
        div.WithCssClassIf(Settings is { IsResponsive: true, IsHeaderFixed: false },  "table-responsive");
        
        var table = HtmlBuilder.Table();
        table.WithCssClass("table");
        table.WithCssClassIf(Settings.IsCompact, "table-sm");
        table.WithCssClassIf(Settings.ShowBorder, "table-bordered");
        table.WithCssClassIf(Settings.ShowRowHover, "table-hover");
        table.WithCssClassIf(Settings.ShowRowStriped, "table-striped");
        table.WithCssClassIf(Settings.IsHeaderFixed, "table-fixed-header");

        table.Append(await Header.GetHtmlBuilderAsync());
        table.Append(await Body.GetHtmlBuilderAsync());

        div.Append(table);
        
        return div;
    }
}