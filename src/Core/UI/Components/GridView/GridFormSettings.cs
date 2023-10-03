using JJMasterData.Commons.Localization;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
internal class GridFormSettings
{
    private readonly IHttpContext _currentContext;
    private readonly IStringLocalizer<JJMasterDataResources> _stringLocalizer;

    private const string TableTotalPerPage = "grid-view-table-regperpage";
    private const string TableTotalPaginationButtons = "grid-view-table-totalpagebuttons";
    private const string TableBorder = "grid-view-table-border";
    private const string TableRowsStriped = "grid-view-table-rowstriped";
    private const string TableRowHover = "grid-view-table-rowhover";
    private const string TableIsHeaderFixed = "grid-view-table-header-fixed";

    public GridFormSettings(IHttpContext currentContext, IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        _currentContext = currentContext;
        _stringLocalizer = stringLocalizer;
    }
    
    public GridSettings LoadFromForm()
    {
        var gridSettings = new GridSettings();
        var tableRegPerPage = _currentContext.Request[TableTotalPerPage];
        var tableTotalPageButtons = _currentContext.Request[TableTotalPaginationButtons];
        var tableBorder = _currentContext.Request[TableBorder];
        var tableRowsStriped = _currentContext.Request[TableRowsStriped];
        var tableRowHover = _currentContext.Request[TableRowHover];
        var tableIsHeaderFixed = _currentContext.Request[TableIsHeaderFixed];

        if (int.TryParse(tableRegPerPage, out var totalPerPage))
            gridSettings.RecordsPerPage = totalPerPage;

        if (int.TryParse(tableTotalPageButtons, out var totalPaggingButtons))
            gridSettings.TotalPaginationButtons = totalPaggingButtons;

        gridSettings.ShowBorder = "true".Equals(tableBorder);
        gridSettings.ShowRowStriped = "true".Equals(tableRowsStriped);
        gridSettings.ShowRowHover = "true".Equals(tableRowHover);
        gridSettings.IsHeaderFixed = "true".Equals(tableIsHeaderFixed);

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
            div.AppendHiddenInput(TableTotalPerPage, gridSettings.RecordsPerPage.ToString());
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
                option.WithAttributeIf(gridSettings.RecordsPerPage == page, "selected", "selected");
                option.WithValue(page.ToString());
                option.AppendText(page.ToString());
            });
        }

        return select;
    }

    private HtmlBuilder GetDataToggleElement(string name, bool isChecked)
    {
        var checkbox = new JJCheckBox(_currentContext.Request.Form, _stringLocalizer)
        {
            Name = name,
            IsChecked = isChecked,
            IsSwitch = true,
            SwitchSize = CheckBoxSwitchSize.Medium
        };

        return checkbox.GetHtmlBuilder();
    }
}