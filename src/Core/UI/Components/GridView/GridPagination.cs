#nullable enable

using System;
using System.Collections.Generic;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal class GridPagination
{
    private JJGridView GridView { get;  }
    private IStringLocalizer<MasterDataResources> StringLocalizer { get;  }
    private int _totalPages;
    private int _totalButtons;
    private int _startButtonIndex;
    private int _endButtonIndex;

    public GridPagination(JJGridView gridView)
    {
        StringLocalizer = gridView.StringLocalizer;
        GridView = gridView;
    }

    public HtmlBuilder GetHtmlBuilder()
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
                div.AppendDiv(div =>
                {
                    div.WithCssClass("col-sm-9");
                    div.AppendDiv(div =>
                    {
                        div.WithCssClass("d-flex");
                        div.AppendDiv(div =>
                        {
                            div.Append(GetPaginationHtmlBuilder());
                        });
                    });
                });
                div.Append(GetTotalRecordsHtmlBuilder());
            });

        return html;
    }

    private HtmlBuilder GetPaginationHtmlBuilder()
    {
        var ul = new HtmlBuilder(HtmlTag.Ul);
        ul.WithCssClass("pagination");

        if (_startButtonIndex > _totalButtons)
        {
            ul.Append(GetPageButton(1, IconType.AngleDoubleLeft, StringLocalizer["First page"]));
            ul.Append(GetPageButton(_startButtonIndex - 1, IconType.AngleLeft, StringLocalizer["Previous page"]));
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
                StringLocalizer["Next page"]));
            ul.Append(GetPageButton(_totalPages, IconType.AngleDoubleRight, StringLocalizer["Last page"]));
        }

        var showJumpToPage = _endButtonIndex <= _totalPages || _startButtonIndex > _totalButtons;
        
        if (showJumpToPage)
        {
            ul.AppendRange(GetJumpToPageButtons());
        }
        
        return ul;
    }

    private IEnumerable<HtmlBuilder> GetJumpToPageButtons()
    {
        var jumpToPageName = GridView.Name + "-jump-to-page-input";
        var textBox = GridView.ComponentFactory.Controls.TextGroup.Create();
        
        textBox.Name = jumpToPageName;
        textBox.MinValue = 1;
        textBox.MaxValue = _totalPages;
        textBox.InputType = InputType.Number;
        
        textBox.Attributes["style"] = "display:none";
        
        textBox.Attributes["onfocusout"] = GridView.Scripts.GetJumpToPageScript();
        textBox.PlaceHolder = StringLocalizer["Jump to page..."];
        textBox.CssClass += " pagination-jump-to-page-input";


        yield return new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("page-item")
            .Append(textBox.GetHtmlBuilder())
            .AppendDiv(div =>
            {
                div.WithCssClass("invalid-feedback");
                div.AppendText(StringLocalizer["Page must be between 1 and {0}.", _totalPages]);
            });

        yield return new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("page-item")
            .Append(new JJLinkButton(GridView.StringLocalizer)
            {
                ShowAsButton = true,
                Icon = IconType.SolidMagnifyingGlassArrowRight,
                CssClass = "pagination-jump-to-page-button",
                OnClientClick = $"GridViewHelper.showJumpToPage('{jumpToPageName}')"
            }.GetHtmlBuilder());
    }
    
    private HtmlBuilder GetPageButton(int page, IconType? icon = null, string? tooltip = null)
    {
        var li = new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("page-item")
            .WithCssClassIf(page == GridView.CurrentPage, "active")
            .Append(HtmlTag.A, a =>
            {
                a.WithCssClass("page-link");
                a.WithAttribute("style", "cursor:pointer; cursor:hand;");
                a.WithToolTip(tooltip);
                a.WithAttribute("onclick", $"javascript:{GridView.Scripts.GetPaginationScript(page)}");
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
                    span.AppendText($" {GridView.TotalOfRecords:N0} ");
                    span.AppendText(StringLocalizer["record(s)"]);
                });
            }
            else
            {
                var firstPageNumber = GridView.CurrentSettings.RecordsPerPage * GridView.CurrentPage -
                    GridView.CurrentSettings.RecordsPerPage + 1;

                var lastPageNumber =
                    GridView.CurrentSettings.RecordsPerPage * GridView.CurrentPage > GridView.TotalOfRecords
                        ? GridView.TotalOfRecords
                        : GridView.CurrentSettings.RecordsPerPage * GridView.CurrentPage;

                label.AppendText($"{firstPageNumber}-{lastPageNumber} {StringLocalizer["From"]}");

                label.Append(HtmlTag.Span, span =>
                {
                    span.WithAttribute("id", $"{GridView.Name}_totrows");
                    span.AppendText(StringLocalizer["{0} records", GridView.TotalOfRecords]);
                });
            }

            label.Append(HtmlTag.Br);

            if(_endButtonIndex <= _totalPages)
                label.AppendText(StringLocalizer["{0} pages",_totalPages]);
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
        string multipleRecordsSelected = GridView.StringLocalizer["{0} selected records", selectedValues.Count];

        var span = new HtmlBuilder(HtmlTag.Span);
        span.WithAttribute("id", $"selected-text-{GridView.Name}");
        span.WithAttribute("no-record-selected-label", noRecordSelected);
        span.WithAttribute("one-record-selected-label", oneRecordSelected);
        span.WithAttribute("multiple-records-selected-label", StringLocalizer["{0} selected records"]);

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