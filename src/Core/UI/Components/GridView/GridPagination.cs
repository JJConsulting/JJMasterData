using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.Localization;
using System;

namespace JJMasterData.Core.Web.Components;

internal class GridPagination
{
    private JJGridView GridView { get; set; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; set; }
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
        _totalPages = (int)Math.Ceiling(GridView.TotalRecords / (double)GridView.CurrentSettings.TotalPerPage);
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
                    span.AppendText($" {GridView.TotalRecords:N0} ");
                    span.AppendText(StringLocalizer["record(s)"]);
                });
            }
            else
            {
                label.AppendText(GridView.CurrentSettings.TotalPerPage * GridView.CurrentPage -
                    GridView.CurrentSettings.TotalPerPage + 1);
                label.AppendText("-");

                if (GridView.CurrentSettings.TotalPerPage * GridView.CurrentPage > GridView.TotalRecords)
                    label.AppendText(GridView.TotalRecords);
                else
                    label.AppendText(GridView.CurrentSettings.TotalPerPage * GridView.CurrentPage);

                label.AppendText($" {StringLocalizer["From"]}");
                label.Append(HtmlTag.Span, span =>
                {
                    span.WithAttribute("id", $"{GridView.Name}_totrows");
                    span.AppendText($"&nbsp;{GridView.TotalRecords:N0}");
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
        span.WithAttribute("id", $"selectedtext_{GridView.Name}");
        span.WithAttribute("noSelStr", noRecordSelected);
        span.WithAttribute("oneSelStr", oneRecordSelected);
        span.WithAttribute("paramSelStr", StringLocalizer["{0} selected records"]);

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