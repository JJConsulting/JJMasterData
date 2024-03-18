using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
internal class GridSettingsForm(
    string name,
    IHttpContext currentContext,
    IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private readonly string _tableTotalPerPage = $"{name}-table-regperpage";
    private readonly string _tableTotalPaginationButtons = $"{name}-table-totalpagebuttons";
    private readonly string _tableBorder = $"{name}-table-border";
    private readonly string _tableRowsStriped = $"{name}-table-rowstriped";
    private readonly string _tableRowHover = $"{name}-table-rowhover";
    private readonly string _tableIsHeaderFixed = $"{name}-table-header-fixed";
    private readonly string _tableIsCompact = $"{name}-table-is-compact";

    public GridSettings LoadFromForm()
    {
        var gridSettings = new GridSettings();
        var tableRegPerPage = currentContext.Request[_tableTotalPerPage];
        var tableTotalPageButtons = currentContext.Request[_tableTotalPaginationButtons];
        var tableBorder = currentContext.Request[_tableBorder];
        var tableRowsStriped = currentContext.Request[_tableRowsStriped];
        var tableRowHover = currentContext.Request[_tableRowHover];
        var tableIsHeaderFixed = currentContext.Request[_tableIsHeaderFixed];
        var tableIsCompact = currentContext.Request[_tableIsCompact];

        if (int.TryParse(tableRegPerPage, out var totalPerPage))
            gridSettings.RecordsPerPage = totalPerPage;

        if (int.TryParse(tableTotalPageButtons, out var totalPaggingButtons))
            gridSettings.TotalPaginationButtons = totalPaggingButtons;

        gridSettings.ShowBorder = StringManager.ParseBool(tableBorder);
        gridSettings.ShowRowStriped = StringManager.ParseBool(tableRowsStriped);
        gridSettings.ShowRowHover = StringManager.ParseBool(tableRowHover);
        gridSettings.IsHeaderFixed = StringManager.ParseBool(tableIsHeaderFixed);
        gridSettings.IsCompact = StringManager.ParseBool(tableIsCompact);
        return gridSettings;
    }

    internal bool HasFormValues() =>
        currentContext.Request[_tableTotalPerPage] != null;

    internal HtmlBuilder GetHtmlBuilder(bool isPaginationEnabled, GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row")
            .AppendHiddenInput(_tableTotalPaginationButtons, gridSettings.TotalPaginationButtons.ToString())
            .AppendHiddenInput(_tableIsHeaderFixed, gridSettings.IsHeaderFixed ? "1" : "0");

        if (isPaginationEnabled)
        {
            div.Append(GetPaginationHtml(gridSettings));
        }
        else
        {
            div.AppendHiddenInput(_tableTotalPerPage, gridSettings.RecordsPerPage.ToString());
        }

        div.Append(GetShowBorderHtml(gridSettings));
        div.Append(GetShowRowsStripedHtml(gridSettings));
        div.Append(GetHighlightLineHtml(gridSettings));
        div.Append(GetIsCompactHtml(gridSettings));

        return div;
    }

    private HtmlBuilder GetHighlightLineHtml(GridSettings gridSettings)
    {
        return GetDataToggleElement(_tableRowHover, stringLocalizer["Highlight line on mouseover"],
            gridSettings.ShowRowHover).WithCssClass("col-sm-2");
    }

    private HtmlBuilder GetIsCompactHtml(GridSettings gridSettings)
    {
        return GetDataToggleElement(_tableIsCompact, stringLocalizer["Compact mode"], gridSettings.IsCompact)
            .WithCssClass("col-sm-2");
    }

    private HtmlBuilder GetShowRowsStripedHtml(GridSettings gridSettings)
    {
        return GetDataToggleElement(_tableRowsStriped, stringLocalizer["Show rows striped"],
            gridSettings.ShowRowStriped).WithCssClass("col-sm-2");
    }

    private HtmlBuilder GetShowBorderHtml(GridSettings gridSettings)
    {
        return GetDataToggleElement(_tableBorder, stringLocalizer["Show table border"], gridSettings.ShowBorder)
            .WithCssClass("col-sm-2");
    }

    private HtmlBuilder GetPaginationHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder()
            .Append(HtmlTag.Label, label =>
            {
                label.WithCssClass("form-label");
                label.WithAttribute("for", _tableTotalPerPage);
                label.AppendText(stringLocalizer["Records per page"]);
            })
            .Append(GetTotalPerPageSelectElement(gridSettings));

        return div;
    }

    private HtmlBuilder GetTotalPerPageSelectElement(GridSettings gridSettings)
    {
        var div = new Div();
        div.WithCssClass("col-sm-3");
        var select = new HtmlBuilder(HtmlTag.Select)
            .WithCssClass("form-control form-select")
            .WithNameAndId(_tableTotalPerPage);

        for (var i = 1; i < 11; i++)
        {
            var page = i * 5;
            select.Append(HtmlTag.Option, option =>
            {
                option.WithAttributeIf(gridSettings.RecordsPerPage == page, "selected", "selected");
                option.WithValue(page.ToString());
                option.AppendText(page.ToString());
            });
        }

        div.Append(select);

        return div;
    }

    private HtmlBuilder GetDataToggleElement(string name, string label, bool isChecked)
    {
        var checkbox = new JJCheckBox(currentContext.Request.Form, stringLocalizer)
        {
            Name = name,
            IsChecked = isChecked,
            Text = label,
            IsSwitch = true,
            SwitchSize = CheckBoxSwitchSize.Default
        };

        return checkbox.GetHtmlBuilder();
    }
}