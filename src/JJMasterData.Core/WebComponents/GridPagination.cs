using System;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

internal class GridPagination
{
    private JJGridView GridView { get; set; }

    private int _totalPages;
    private int _totalButtons;
    private int _startButtonIndex;
    private int _endButtonIndex;

    public GridPagination(JJGridView gridView)
    {
        GridView = gridView;
    }

    public HtmlBuilder GetHtmlElement()
    {
        _totalPages = (int)Math.Ceiling(GridView.TotalRecords / (double)GridView.CurrentUI.TotalPerPage);
        _totalButtons = GridView.CurrentUI.TotalPaginationButtons;
        _startButtonIndex = (int)Math.Floor((GridView.CurrentPage - 1) / (double)_totalButtons) * _totalButtons + 1;
        _endButtonIndex = _startButtonIndex + _totalButtons;

        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClassIf(BootstrapHelper.Version > 3, "container-fluid p-0")
            .AppendElement(HtmlTag.Div, div =>
            {
                div.WithCssClass("row justify-content-between");
                div.AppendElement(HtmlTag.Div, div =>
                {
                    div.WithCssClass("col-sm-9");
                    div.AppendElement(GetPaginationHtmlElement());
                });
                div.AppendElement(GetTotalRecordsHtmlElement());
            });

        return html;
    }

    private HtmlBuilder GetPaginationHtmlElement()
    {
        var ul = new HtmlBuilder(HtmlTag.Ul);
        ul.WithCssClass("pagination");

        if (_startButtonIndex > _totalButtons)
        {
            ul.AppendElement(GetPageButton(1, IconType.AngleDoubleLeft, Translate.Key("First record")));
            ul.AppendElement(GetPageButton(_startButtonIndex - 1, IconType.AngleLeft));
        }

        for (int i = _startButtonIndex; i < _endButtonIndex; i++)
        {
            if (i > _totalPages || _totalPages <= 1)
                break;
            ul.AppendElement(GetPageButton(i));
        }


        if (_endButtonIndex <= _totalPages)
        {
            ul.AppendElement(GetPageButton(_endButtonIndex, IconType.AngleRight,
                $"{_totalPages} {Translate.Key("pages")}"));
            ul.AppendElement(GetPageButton(_totalPages, IconType.AngleDoubleRight, Translate.Key("Last record")));
        }

        return ul;
    }

    private HtmlBuilder GetPageButton(int page, IconType? icon = null, string tooltip = null)
    {
        var li = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("page-item")
            .WithCssClassIf(page == GridView.CurrentPage, "active")
            .AppendElement(HtmlTag.A, a =>
            {
                a.WithCssClass("page-link");
                a.WithAttribute("style", "cursor:pointer; cursor:hand;");
                a.WithToolTip(tooltip);
                a.WithAttribute("onclick", GetDoPaginationScript(page));
                if (icon != null)
                {
                    a.AppendElement(new JJIcon(icon.Value));
                }
                else
                {
                    a.AppendText(page.ToString());
                }
            });

        return li;
    }

    private string GetDoPaginationScript(int page)
    {
        string name = GridView.Name;
        string enableAjax = GridView.EnableAjax ? "true" : "false";

        return $"javascript:jjview.doPagination('{name}', {enableAjax}, {page})";
    }

    private HtmlBuilder GetTotalRecordsHtmlElement()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass($"col-sm-3 {BootstrapHelper.TextRight}");
        div.AppendElement(HtmlTag.Label, label =>
        {
            label.WithAttribute("id", $"infotext_{GridView.Name}");
            label.WithCssClass("small");
            label.AppendText(Translate.Key("Showing"));
            label.AppendText(" ");
            
            if (_totalPages <= 1)
            {
                label.AppendElement(HtmlTag.Span, span =>
                {
                    span.WithAttribute("id", $"{GridView.Name}_totrows");
                    span.AppendText($" {GridView.TotalRecords:N0} ");
                    span.AppendText(Translate.Key("record(s)"));
                });
            }
            else
            {
                label.AppendText(GridView.CurrentUI.TotalPerPage * GridView.CurrentPage -
                    GridView.CurrentUI.TotalPerPage + 1);
                label.AppendText("-");

                if (GridView.CurrentUI.TotalPerPage * GridView.CurrentPage > GridView.TotalRecords)
                    label.AppendText(GridView.TotalRecords);
                else
                    label.AppendText(GridView.CurrentUI.TotalPerPage * GridView.CurrentPage);

                label.AppendText($" {Translate.Key("From")}");
                label.AppendElement(HtmlTag.Span, span =>
                {
                    span.WithAttribute("id", $"{GridView.Name}_totrows");
                    span.AppendText($"&nbsp;{GridView.TotalRecords:N0}");
                });
            }
        });

        div.AppendElementIf(GridView.EnableMultSelect, HtmlTag.Br);
        div.AppendElementIf(GridView.EnableMultSelect, GetEnableMultSelectTotalRecords);
        
        return div;
    }

    private HtmlBuilder GetEnableMultSelectTotalRecords()
    {
        var selectedValues = GridView.GetSelectedGridValues();
        string noRecordSelected = Translate.Key("No record selected");
        string oneRecordSelected = Translate.Key("A selected record");
        string multipleRecordsSelected = Translate.Key("{0} selected records", selectedValues?.Count);

        var span = new HtmlBuilder(HtmlTag.Span);
        span.WithAttribute("id", $"selectedtext_{GridView.Name}");
        span.WithAttribute("noSelStr", noRecordSelected);
        span.WithAttribute("oneSelStr", oneRecordSelected);
        span.WithAttribute("paramSelStr", Translate.Key("{0} selected records"));

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