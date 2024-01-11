using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
internal class GridFormSettings(IHttpContext currentContext, IStringLocalizer<MasterDataResources> stringLocalizer)
{
    private const string TableTotalPerPage = "grid-view-table-regperpage";
    private const string TableTotalPaginationButtons = "grid-view-table-totalpagebuttons";
    private const string TableBorder = "grid-view-table-border";
    private const string TableRowsStriped = "grid-view-table-rowstriped";
    private const string TableRowHover = "grid-view-table-rowhover";
    private const string TableIsHeaderFixed = "grid-view-table-header-fixed";
    private const string TableIsCompact = "grid-view-table-is-compact";
    public GridSettings LoadFromForm()
    {
        var gridSettings = new GridSettings();
        var tableRegPerPage = currentContext.Request[TableTotalPerPage];
        var tableTotalPageButtons = currentContext.Request[TableTotalPaginationButtons];
        var tableBorder = currentContext.Request[TableBorder];
        var tableRowsStriped = currentContext.Request[TableRowsStriped];
        var tableRowHover = currentContext.Request[TableRowHover];
        var tableIsHeaderFixed = currentContext.Request[TableIsHeaderFixed];
        var tableIsCompact = currentContext.Request[TableIsCompact];

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
            div.Append(GetPaginationHtml(gridSettings));
        }
        else
        {
            div.AppendHiddenInput(TableTotalPerPage, gridSettings.RecordsPerPage.ToString());
        }

        div.Append(GetShowBorderHtml(gridSettings));
        div.Append(GetShowRowsStripedHtml(gridSettings));
        div.Append(GetHighlightLineHtml(gridSettings));
        div.Append(GetIsCompactHtml(gridSettings));
        return div;
    }

    private HtmlBuilder GetHighlightLineHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableRowHover);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Highlight line on mouseover"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(TableRowHover, gridSettings.ShowRowHover));
        });
        
        return div;
    }

    private HtmlBuilder GetIsCompactHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableIsCompact);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Is Compact"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(TableIsCompact, gridSettings.IsCompact));
        });
        
        return div;
    }
    
    private HtmlBuilder GetShowRowsStripedHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableRowsStriped);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Show rows striped"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(TableRowsStriped, gridSettings.ShowRowStriped));
        });

        return div;
    }

    private HtmlBuilder GetShowBorderHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableBorder);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Show table border"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(TableBorder, gridSettings.ShowBorder));
        });


        return div;
    }

    private HtmlBuilder GetPaginationHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", TableTotalPerPage);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Records per Page"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-2");
            div.Append(GetTotalPerPageSelectElement(gridSettings));
        });
        div.Append(HtmlTag.Div, div => { div.WithCssClass("col-sm-6"); });

        return div;
    }

    private static HtmlBuilder GetTotalPerPageSelectElement(GridSettings gridSettings)
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
        var checkbox = new JJCheckBox(currentContext.Request.Form, stringLocalizer)
        {
            Name = name,
            IsChecked = isChecked,
            IsSwitch = true,
            SwitchSize = CheckBoxSwitchSize.Medium
        };

        return checkbox.GetHtmlBuilder();
    }
}