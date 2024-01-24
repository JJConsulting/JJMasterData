using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
internal class GridSettingsForm(string name, IHttpContext currentContext, IStringLocalizer<MasterDataResources> stringLocalizer)
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
            .WithCssClass($"{(BootstrapHelper.Version == 3 ? "form-horizontal" : string.Empty)}")
            .WithAttribute("role", "form")
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
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", _tableRowHover);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Highlight line on mouseover"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(_tableRowHover, gridSettings.ShowRowHover));
        });
        
        return div;
    }

    private HtmlBuilder GetIsCompactHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", _tableIsCompact);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Compact Mode"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(_tableIsCompact, gridSettings.IsCompact));
        });
        
        return div;
    }
    
    private HtmlBuilder GetShowRowsStripedHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", _tableRowsStriped);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Show rows striped"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(_tableRowsStriped, gridSettings.ShowRowStriped));
        });

        return div;
    }

    private HtmlBuilder GetShowBorderHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", _tableBorder);
                label.WithCssClass("col-sm-4");
                label.AppendText(stringLocalizer["Show table border"]);
            });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-8");
            div.Append(GetDataToggleElement(_tableBorder, gridSettings.ShowBorder));
        });


        return div;
    }

    private HtmlBuilder GetPaginationHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass($"{BootstrapHelper.FormGroup} row")
            .Append(HtmlTag.Label, label =>
            {
                label.WithAttribute("for", _tableTotalPerPage);
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

    private HtmlBuilder GetTotalPerPageSelectElement(GridSettings gridSettings)
    {
        var select = new HtmlBuilder(HtmlTag.Select)
            .WithCssClass("form-control form-select")
            .WithNameAndId(_tableTotalPerPage);

        for (var i = 1; i < 7; i++)
        {
            var page = i * 5;
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