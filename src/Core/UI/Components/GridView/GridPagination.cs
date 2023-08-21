using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.Localization;
using System;

namespace JJMasterData.Core.Web.Components;

internal class GridPagination
{
    private JJGridView GridView { get;  }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get;  }
    private int _totalPages;
    private int _totalButtons;
    private int _startButtonIndex;
    private int _endButtonIndex;

    public GridPagination(JJGridView gridView)
    {
        StringLocalizer = gridView.StringLocalizer;
        GridView = gridView;
    }

    public HtmlBuilder GetHtmlElement()
    {
        _totalPages = (int)Math.Ceiling(GridView.TotalOfRecords  / (double)GridView.CurrentSettings.RecordsPerPage);
        _totalButtons = GridView.CurrentSettings.TotalPaginationButtons;
        _startButtonIndex = (int)Math.Floor((GridView.CurrentPage - 1) / (double)_totalButtons) * _totalButtons + 1;
        _endButtonIndex = _startButtonIndex + _totalButtons;

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClassIf(BootstrapHelper.Version > 3, "container-fluid p-0")
            .Append(HtmlTag.Div, div =>
            {
                div.WithCssClass("row justify-content-between");
                div.Append(HtmlTag.Div, div =>
                {
                    div.WithCssClass("col-sm-9");
                    div.Append(GetPaginationHtmlElement());
                });
                div.Append(GetTotalRecordsHtmlElement());
            });

        return html;
    }

    private HtmlBuilder GetPaginationHtmlElement()
    {
        var ul = new HtmlBuilder(HtmlTag.Ul);
        ul.WithCssClass("pagination");

        if (_startButtonIndex > _totalButtons)
        {
            ul.Append(GetPageButton(1, IconType.AngleDoubleLeft, StringLocalizer["First record"]));
            ul.Append(GetPageButton(_startButtonIndex - 1, IconType.AngleLeft));
        }

        for (int i = _startButtonIndex; i < _endButtonIndex; i++)
        {
            if (i > _totalPages || _totalPages <= 1)
                break;
            ul.Append(GetPageButton(i));
        }


        if (_endButtonIndex <= _totalPages)
        {
            ul.Append(GetPageButton(_endButtonIndex, IconType.AngleRight,
                $"{_totalPages} {StringLocalizer["pages"]}"));
            ul.Append(GetPageButton(_totalPages, IconType.AngleDoubleRight, StringLocalizer["Last record"]));
        }

        return ul;
    }

    private HtmlBuilder GetPageButton(int page, IconType? icon = null, string tooltip = null)
    {
        var li = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("page-item")
            .WithCssClassIf(page == GridView.CurrentPage, "active")
            .Append(HtmlTag.A, a =>
            {
                a.WithCssClass("page-link");
                a.WithAttribute("style", "cursor:pointer; cursor:hand;");
                a.WithToolTip(tooltip);
                a.WithAttribute("onclick", "javascript:" + GridView.Scripts.GetPaginationScript(page));
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
    
    private HtmlBuilder GetTotalRecordsHtmlElement()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass($"col-sm-3 {BootstrapHelper.TextRight}");
        div.Append(HtmlTag.Label, label =>
        {
            label.WithAttribute("id", $"infotext_{GridView.Name}");
            label.WithCssClass("small");
            label.AppendText(StringLocalizer["Showing"]);
            label.AppendText(" ");
            
            if (_totalPages <= 1)
            {
                label.Append(HtmlTag.Span, span =>
                {
                    span.WithAttribute("id", $"{GridView.Name}_totrows");
                    span.AppendText($" {GridView?.TotalOfRecords:N0} ");
                    span.AppendText(StringLocalizer["record(s)"]);
                });
            }
            else
            {
                label.AppendText(GridView.CurrentSettings.RecordsPerPage * GridView.CurrentPage -
                    GridView.CurrentSettings.RecordsPerPage + 1);
                label.AppendText("-");

                if (GridView.CurrentSettings.RecordsPerPage * GridView.CurrentPage > GridView?.TotalOfRecords)
                    label.AppendText(GridView.TotalOfRecords);
                else
                    label.AppendText(GridView.CurrentSettings.RecordsPerPage * GridView.CurrentPage);

                label.AppendText($" {StringLocalizer["From"]}");
                label.Append(HtmlTag.Span, span =>
                {
                    span.WithAttribute("id", $"{GridView.Name}_totrows");
                    span.AppendText($"&nbsp;{GridView.TotalOfRecords:N0}");
                });
            }
        });

        div.AppendIf(GridView.EnableMultiSelect, HtmlTag.Br);
        div.AppendIf(GridView.EnableMultiSelect, GetEnableMultSelectTotalRecords);
        
        return div;
    }

    private HtmlBuilder GetEnableMultSelectTotalRecords()
    {
        var selectedValues = GridView.GetSelectedGridValues();
        string noRecordSelected = GridView.StringLocalizer["No record selected"];
        string oneRecordSelected = GridView.StringLocalizer["A selected record"];
        string multipleRecordsSelected = GridView.StringLocalizer["{0} selected records", selectedValues?.Count];

        var span = new HtmlBuilder(HtmlTag.Span);
        span.WithAttribute("id", $"selected-text-{GridView.Name}");
        span.WithAttribute("no-record-selected-label", noRecordSelected);
        span.WithAttribute("one-record-selected-label", oneRecordSelected);
        span.WithAttribute("multiple-records-selected-label", StringLocalizer["{0} selected records"]);

        if (selectedValues == null || selectedValues.Count == 0)
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