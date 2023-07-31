using JJMasterData.Commons.Localization;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
internal class GridFormSettings
{
    private readonly IHttpContext _currentContext;
    private readonly IStringLocalizer<JJMasterDataResources> _stringLocalizer;

    private const string TableTotalPerPage = "table_regperpage";
    private const string TableTotalPaginationButtons = "table_totalpagebuttons";
    private const string TableBorder = "table_border";
    private const string TableRowsStriped = "table_rowstriped";
    private const string TableRowHover = "table_rowhover";
    private const string TableIsHeaderFixed = "table_headerfixed";

    public GridFormSettings(IHttpContext currentContext, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        _currentContext = currentContext;
        _stringLocalizer = stringLocalizer;
    }
    
    public GridSettings LoadFromForm()
    {
        var gridSettings = new GridSettings();
        string tableRegPerPage = _currentContext.Request[TableTotalPerPage];
        string tableTotalPageButtons = _currentContext.Request[TableTotalPaginationButtons];
        string tableBorder = _currentContext.Request[TableBorder];
        string tableRowsStriped = _currentContext.Request[TableRowsStriped];
        string tableRowHover = _currentContext.Request[TableRowHover];
        string tableIsHeaderFixed = _currentContext.Request[TableIsHeaderFixed];

        if (int.TryParse(tableRegPerPage, out int totalPerPage))
            gridSettings.TotalPerPage = totalPerPage;

        if (int.TryParse(tableTotalPageButtons, out int totalPaggingButtons))
            gridSettings.TotalPaginationButtons = totalPaggingButtons;

        gridSettings.ShowBorder = "1".Equals(tableBorder);
        gridSettings.ShowRowStriped = "1".Equals(tableRowsStriped);
        gridSettings.ShowRowHover = "1".Equals(tableRowHover);
        gridSettings.IsHeaderFixed = "1".Equals(tableIsHeaderFixed);

        return gridSettings;
    }

    internal static bool HasFormValues(IHttpContext currentContext) =>
        currentContext.Request[TableTotalPerPage] != null;

    internal HtmlBuilder GetHtmlElement(bool isPaginationEnabled, GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{(BootstrapHelper.Version == 3 ? "form-horizontal" : string.Empty)}")
            .WithAttribute("role", "form")
            .AppendHiddenInput(TableTotalPaginationButtons, gridSettings.TotalPaginationButtons.ToString())
            .AppendHiddenInput(TableIsHeaderFixed, gridSettings.IsHeaderFixed ? "1" : "0");

        if (isPaginationEnabled)
        {
            div.Append(GetPaginationElement(gridSettings));
        }
        else
        {
            div.AppendHiddenInput(TableTotalPerPage, gridSettings.TotalPerPage.ToString());
        }

        div.Append(GetShowBorderElement(gridSettings));
        div.Append(GetShowRowsStripedElement(gridSettings));
        div.Append(GetHighlightLineElement(gridSettings));

        return div;
    }

    private HtmlBuilder GetHighlightLineElement(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableRowHover);
                label.WithCssClass("col-sm-4");
                label.AppendText(_stringLocalizer["Highlight line on mouseover"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(TableRowHover, gridSettings.ShowRowHover));
        });


        return div;
    }

    private HtmlBuilder GetShowRowsStripedElement(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableRowsStriped);
                label.WithCssClass("col-sm-4");
                label.AppendText(_stringLocalizer["Show rows striped"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(TableRowsStriped, gridSettings.ShowRowStriped));
        });

        return div;
    }

    private HtmlBuilder GetShowBorderElement(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableBorder);
                label.WithCssClass("col-sm-4");
                label.AppendText(_stringLocalizer["Show table border"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(TableBorder, gridSettings.ShowBorder));
        });


        return div;
    }

    private HtmlBuilder GetPaginationElement(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableTotalPerPage);
                label.WithCssClass("col-sm-4");
                label.AppendText(_stringLocalizer["Records per Page"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-2");
            div.Append(GetTotalPerPageSelectElement(gridSettings));
        });
        div.Append(HtmlTag.Div, div => { div.WithCssClass("col-sm-6"); });

        return div;
    }

    private HtmlBuilder GetTotalPerPageSelectElement(GridSettings gridSettings)
    {
        var select = new HtmlBuilder(HtmlTag.Select)
            .WithCssClass("form-control form-select")
            .WithNameAndId(TableTotalPerPage);

        for (int i = 1; i < 7; i++)
        {
            int page = i * 5;
            select.Append(HtmlTag.Option, option =>
            {
                option.WithAttributeIf(gridSettings.TotalPerPage == page, "selected", "selected");
                option.WithValue(page.ToString());
                option.AppendText(page.ToString());
            });
        }

        return select;
    }

    private HtmlBuilder GetDataToggleElement(string name, bool isChecked)
    {
        var input = new HtmlBuilder(HtmlTag.Input)
            .WithAttribute("type", "checkbox")
            .WithValue("1")
            .WithCssClass("form-control")
            .WithNameAndId(name)
            .WithAttributeIf(isChecked, "checked", "checked")
            .WithAttribute("data-toggle", "toggle")
            .WithAttribute("data-on", _stringLocalizer["Yes"])
            .WithAttribute("data-off", _stringLocalizer["No"]);

        return input;
    }
}