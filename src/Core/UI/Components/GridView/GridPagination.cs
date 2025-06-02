#nullable enable

using System;
using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class GridPagination(JJGridView gridView)
{
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer = gridView.StringLocalizer;
    private int _totalOfPages;
    private int _totalOfButtons;
    private int _startButtonIndex;
    private int _endButtonIndex;

    public HtmlBuilder GetHtmlBuilder()
    {
        _totalOfPages = gridView.TotalOfPages;
        _totalOfButtons = gridView.CurrentSettings.TotalPaginationButtons;
        _startButtonIndex = (int)Math.Floor((gridView.CurrentPage - 1) / (double)_totalOfButtons) * _totalOfButtons + 1;
        _endButtonIndex = _startButtonIndex + _totalOfButtons;

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClassIf(BootstrapHelper.Version > 3, "container-fluid p-0")
            .Append(HtmlTag.Div, this, static (grid, div) =>
            {
                div.WithCssClass("row justify-content-between");
                div.AppendDiv(grid, static (grid, div) =>
                {
                    div.WithCssClass("col-sm-9");
                    div.AppendDiv(grid, static (grid, div) =>
                    {
                        div.WithCssClass("d-flex");
                        div.AppendDiv(grid, static (grid, div) => div.Append(grid.GetPaginationHtmlBuilder()));
                    });
                });
                div.Append(grid.GetTotalRecordsHtmlBuilder());
            });

        return html;
    }

    private HtmlBuilder GetPaginationHtmlBuilder()
    {
        var ul = new HtmlBuilder(HtmlTag.Ul);
        ul.WithCssClass("pagination");

        if (_startButtonIndex > _totalOfButtons)
        {
            ul.Append(GetPageButton(1, IconType.AngleDoubleLeft, _stringLocalizer["First page"]));
            ul.Append(GetPageButton(_startButtonIndex - 1, IconType.AngleLeft, _stringLocalizer["Previous page"]));
        }

        for (int i = _startButtonIndex; i < _endButtonIndex; i++)
        {
            if (i > _totalOfPages || _totalOfPages <= 1)
                break;
            ul.Append(GetPageButton(i));
        }

        if (_endButtonIndex <= _totalOfPages)
        {
            ul.Append(GetPageButton(_endButtonIndex, IconType.AngleRight,
                _stringLocalizer["Next page"]));
            ul.Append(GetPageButton(_totalOfPages, IconType.AngleDoubleRight, _stringLocalizer["Last page"]));
        }

        var showJumpToPage = _endButtonIndex <= _totalOfPages || _startButtonIndex > _totalOfButtons;

        if (showJumpToPage && BootstrapHelper.Version >= 5)
        {
            ul.AppendRange(GetJumpToPageButtons());
        }

        return ul;
    }

    private IEnumerable<HtmlBuilder> GetJumpToPageButtons()
    {
        var jumpToPageName = gridView.Name + "-jump-to-page-input";
        var textBox = gridView.ComponentFactory.Controls.TextGroup.Create();

        textBox.Name = jumpToPageName;
        textBox.MinValue = 1;
        textBox.MaxValue = _totalOfPages;
        textBox.InputType = InputType.Number;

        textBox.Attributes["style"] = "display:none;width:150px";

        textBox.Attributes["onfocusout"] = gridView.Scripts.GetJumpToPageScript();
        textBox.PlaceHolder = _stringLocalizer["Jump to page..."];
        textBox.CssClass += " pagination-jump-to-page-input";

        yield return new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("page-item")
            .Append(textBox.GetHtmlBuilder())
            .AppendDiv(div =>
            {
                div.WithId(jumpToPageName + "-invalid-feedback");
                div.WithCssClass("invalid-feedback");
                div.AppendText(_stringLocalizer["Page must be between 1 and {0}.", _totalOfPages]);
            });

        yield return new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("page-item")
            .Append(new JJLinkButton
            {
                ShowAsButton = false,
                Icon = IconType.SolidMagnifyingGlassArrowRight,
                CssClass = "btn pagination-jump-to-page-button",
                OnClientClick = $"GridViewHelper.showJumpToPage('{jumpToPageName}')"
            }.GetHtmlBuilder());
    }

    private HtmlBuilder GetPageButton(int page, IconType? icon = null, string? tooltip = null)
    {
        var li = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("page-item")
            .WithCssClassIf(page == gridView.CurrentPage, "active")
            .Append(HtmlTag.A, a =>
            {
                a.WithCssClass("page-link");
                a.WithStyle("cursor:pointer; cursor:hand;");
                a.WithToolTip(tooltip);
                a.WithOnClick($"javascript:{gridView.Scripts.GetPaginationScript(page)}");
                if (icon != null)
                {
                    a.AppendComponent(new JJIcon(icon.Value));
                }
                else
                {
                    a.AppendText(page.ToString());
                }
            });

        return li;
    }

    private HtmlBuilder GetTotalRecordsHtmlBuilder()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass($"col-sm-3 {BootstrapHelper.TextRight} text-muted m-0");

        var span = new HtmlBuilder(HtmlTag.Span);
        span.WithAttribute("id", $"infotext_{gridView.Name}");
        span.WithCssClass("small d-block");
        span.AppendText(_stringLocalizer["Showing"]);
        span.AppendText(" ");

        var totalOfRecords = gridView.TotalOfRecords;

        if (_totalOfPages <= 1)
        {
            var totalOfRecordsHtml = new HtmlBuilder(HtmlTag.Strong);
            totalOfRecordsHtml.WithId($"{gridView.Name}-total-of-records");
            totalOfRecordsHtml.AppendText(totalOfRecords);

            span.Append(totalOfRecordsHtml);
            span.AppendText($" {_stringLocalizer["record(s)"]}");
        }
        else
        {
            var firstPageNumber = gridView.CurrentSettings.RecordsPerPage * gridView.CurrentPage -
                gridView.CurrentSettings.RecordsPerPage + 1;

            var lastPageNumber =
                gridView.CurrentSettings.RecordsPerPage * gridView.CurrentPage > gridView.TotalOfRecords
                    ? gridView.TotalOfRecords
                    : gridView.CurrentSettings.RecordsPerPage * gridView.CurrentPage;

            var pagesHtml = new HtmlBuilder(HtmlTag.Strong);
            pagesHtml.AppendText($"{firstPageNumber}-{lastPageNumber}");

            span.Append(pagesHtml).AppendText($" {_stringLocalizer["from"]}");

            var recordsHtml = new HtmlBuilder(HtmlTag.Strong);
            recordsHtml.WithId($"{gridView.Name}-total-of-records");
            recordsHtml.AppendText(totalOfRecords);

            span.Append(recordsHtml).AppendText($" {_stringLocalizer["records"]}");
        }

        if (_endButtonIndex <= _totalOfPages)
        {
            span.AppendBr();

            var totalOfPagesHtml = new HtmlBuilder(HtmlTag.Strong);
            totalOfPagesHtml.AppendText(_totalOfPages);

            span.Append(totalOfPagesHtml);

            span.AppendText($" {_stringLocalizer["pages"]}");
        }

        div.Append(span);

        if (gridView.EnableMultiSelect)
            div.Append(GetEnableMultSelectTotalRecords());

        return div;
    }

    private HtmlBuilder GetEnableMultSelectTotalRecords()
    {
        var selectedValues = gridView.GetSelectedGridValues();
        string noRecordSelected = gridView.StringLocalizer["No record selected"];
        string oneRecordSelected = gridView.StringLocalizer["One selected record"];
        string multipleRecordsSelected = gridView.StringLocalizer["{0} selected records", selectedValues.Count];

        var span = new HtmlBuilder(HtmlTag.Span);
        span.WithCssClass("small d-block");
        span.WithAttribute("id", $"selected-text-{gridView.Name}");
        span.WithAttribute("no-record-selected-label", noRecordSelected);
        span.WithAttribute("one-record-selected-label", oneRecordSelected);
        span.WithAttribute("multiple-records-selected-label", _stringLocalizer["{0} selected records"]);

        if (selectedValues.Count == 0)
        {
            span.AppendText(noRecordSelected);
        }
        else if (selectedValues.Count == 1)
        {
            span.AppendText(oneRecordSelected);
        }
        else
        {
            span.AppendText(multipleRecordsSelected);
        }

        return span;
    }
}