﻿using JJMasterData.Commons.Localization;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

/// <summary>
/// Class responsible to render the UI on JJGridView
/// </summary>
internal sealed class GridSettingsForm(
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
            .WithCssClass("container")
            .AppendHiddenInput(_tableTotalPaginationButtons, gridSettings.TotalPaginationButtons.ToString())
            .AppendHiddenInput(_tableIsHeaderFixed, gridSettings.IsHeaderFixed ? "1" : "0");

        if (isPaginationEnabled)
        {
            div.AppendDiv(div =>
            {
                div.Append(GetPaginationHtml(gridSettings));
                div.WithCssClass($"row {BootstrapHelper.FormGroup}");
                div.WithCssClass(BootstrapHelper.FormGroup);
            });
        }
        else
        {
            div.AppendHiddenInput(_tableTotalPerPage, gridSettings.RecordsPerPage.ToString());
        }

        div.AppendDiv(div=>
        {
            div.Append(GetShowBorderHtml(gridSettings));
            div.WithCssClass("row");
            div.WithCssClass(BootstrapHelper.FormGroup);
        });
        div.AppendDiv(div=>
        {
            div.Append(GetShowRowsStripedHtml(gridSettings));
            div.WithCssClass("row");
            div.WithCssClass(BootstrapHelper.FormGroup);
        });
        div.AppendDiv(div=>
        {
            div.Append(GetHighlightLineHtml(gridSettings));
            div.WithCssClass("row");
            div.WithCssClass(BootstrapHelper.FormGroup);
        });
        div.AppendDiv(div=>
        {
            div.Append(GetIsCompactHtml(gridSettings));
            div.WithCssClass("row");
            div.WithCssClass(BootstrapHelper.FormGroup);
        });

        return div;
    }

    private HtmlBuilder GetHighlightLineHtml(GridSettings gridSettings)
    {
        return GetDataToggleElement(_tableRowHover, stringLocalizer["Highlight line on mouseover"],
            gridSettings.ShowRowHover).WithCssClass("col-sm-12");
    }

    private HtmlBuilder GetIsCompactHtml(GridSettings gridSettings)
    {
        return GetDataToggleElement(_tableIsCompact, stringLocalizer["Compact mode"], gridSettings.IsCompact)
            .WithCssClass("col-sm-12");
    }

    private HtmlBuilder GetShowRowsStripedHtml(GridSettings gridSettings)
    {
        return GetDataToggleElement(_tableRowsStriped, stringLocalizer["Show rows striped"],
            gridSettings.ShowRowStriped).WithCssClass("col-sm-12");
    }

    private HtmlBuilder GetShowBorderHtml(GridSettings gridSettings)
    {
        return GetDataToggleElement(_tableBorder, stringLocalizer["Show table border"], gridSettings.ShowBorder)
            .WithCssClass("col-sm-12");
    }
    
    private HtmlBuilder GetPaginationHtml(GridSettings gridSettings)
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("col-sm-6");

        if (BootstrapHelper.Version is 3)
        {
            div.Append(GetPaginationLabel());
            div.Append(GetPaginationSelect(gridSettings));
        }
        else
        {
            div.AppendDiv(div =>
            {
                div.WithCssClass("form-floating");
                div.Append(GetPaginationSelect(gridSettings));
                div.Append(GetPaginationLabel());
            });
        }

        return div;
    }

    private HtmlBuilder GetPaginationLabel()
    {
        var label = new HtmlBuilder(HtmlTag.Label);
        label.WithCssClass("form-label");
        label.WithAttribute("for", _tableTotalPerPage);
        label.AppendText(stringLocalizer["Records per page"]);
        return label;
    }

    private HtmlBuilder GetPaginationSelect(GridSettings gridSettings)
    {
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

        return select;
    }

    private HtmlBuilder GetDataToggleElement(string name, string label, bool isChecked)
    {
        var checkbox = new JJCheckBox(currentContext.Request.Form, stringLocalizer)
        {
            Name = name,
            IsChecked = isChecked,
            Text = label,
            Layout = CheckboxLayout.Switch
        };

        return checkbox.GetHtmlBuilder();
    }
}