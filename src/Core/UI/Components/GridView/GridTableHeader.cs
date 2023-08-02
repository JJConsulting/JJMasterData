using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;
using JJMasterData.Commons.Data.Entity;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components.Scripts;
using JJMasterData.Core.Web.Html;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

internal class GridTableHeader
{
    private JJGridView GridView { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }
    public GridTableHeader(JJGridView gridView)
    {
        GridView = gridView;
        StringLocalizer = GridView.StringLocalizer;
    }

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Thead);
        if (GridView.DataSource?.Rows.Count == 0 && !GridView.ShowHeaderWhenEmpty)
            return html;

        await html.AppendAsync(HtmlTag.Tr, async tr =>
        {
            tr.AppendIf(GridView.EnableMultiSelect, GetMultSelectThHtmlElement);
            await tr.AppendRangeAsync(GetVisibleFieldsThList());
            tr.AppendRange(GetActionsThList());
        });

        return html;
    }

    private IEnumerable<HtmlBuilder> GetActionsThList()
    {
        var basicActions = GridView.GridActions.OrderBy(x => x.Order).ToList();
        var actions = basicActions.FindAll(x => x.IsVisible && !x.IsGroup);
        var actionsWithGroupCount = basicActions.Count(x => x.IsVisible && x.IsGroup);

        foreach (var action in actions)
        {
            var th = new HtmlBuilder(HtmlTag.Th);
            th.AppendTextIf(action.ShowTitle, action.Text);
            yield return th;
        }

        if (actionsWithGroupCount > 0)
        {
            yield return new HtmlBuilder(HtmlTag.Th);
        }
    }

    private async IAsyncEnumerable<HtmlBuilder> GetVisibleFieldsThList()
    {
        await foreach (var field in GridView.GetVisibleFieldsAsync())
        {
            var th = new HtmlBuilder(HtmlTag.Th);
            string style = GetFieldStyle(field);

            th.WithAttributeIf(!string.IsNullOrEmpty(style), "style", style);
            th.Append(HtmlTag.Span, span =>
            {
                if (GridView.EnableSorting && field.DataBehavior != FieldBehavior.Virtual)
                {
                    SetSortAttributes(span, field);
                }
                span.Append(HtmlTag.Span, span =>
                {
                    if (!string.IsNullOrEmpty(field.HelpDescription))
                    {
                        span.WithToolTip(field.HelpDescription);
                    }

                    span.AppendText(StringLocalizer[field.Label]);
                });
            });

            if (!string.IsNullOrEmpty(GridView.CurrentOrder))
            {
                foreach (string orderField in GridView.CurrentOrder.Split(','))
                {
                    string order = orderField.Trim();
                    if (string.IsNullOrWhiteSpace(order))
                        break;

                    if (order.StartsWith("["))
                    {
                        order = order.Replace("[", "");
                        order = order.Replace("]", "");
                    }

                    if (order.Equals(field.Name + " DESC"))
                        th.Append(GetDescendingIcon());
                    else if (order.Equals(field.Name + " ASC") || order.Equals(field.Name))
                        th.Append(GetAscendingIcon());
                }
            }
            else
            {
                if (field.Name.EndsWith("::DESC"))
                    th.Append(GetDescendingIcon());
                else if (field.Name.EndsWith("::ASC"))
                    th.Append(GetAscendingIcon());
            }

            bool isAppliedFilter = GridView.GetCurrentFilterAsync() != null &&
                                   field.Filter.Type != FilterMode.None &&
                                   !GridView.RelationValues.ContainsKey(field.Name) &&
                                   ((await GridView.GetCurrentFilterAsync()).ContainsKey(field.Name) ||
                                    (await GridView.GetCurrentFilterAsync()).ContainsKey(field.Name + "_from"));

            if (isAppliedFilter)
            {
                th.AppendText("&nbsp;");
                th.Append(new JJIcon("fa fa-filter").GetHtmlBuilder()
                    .WithToolTip(StringLocalizer["Applied filter"]));
            }

            yield return th;
        }
    }

    private void SetSortAttributes(HtmlBuilder span, FormElementField field)
    {
        span.WithCssClass("jjenable-sorting");
        span.WithAttribute("onclick", GridView.Scripts.GetSortingScript(GridView,field.Name));
    }

    private HtmlBuilder GetAscendingIcon() => new JJIcon("fa fa-sort-amount-asc").GetHtmlBuilder()
        .WithToolTip(StringLocalizer["Descending order"]);

    private HtmlBuilder GetDescendingIcon() => new JJIcon("fa fa-sort-amount-desc").GetHtmlBuilder()
        .WithToolTip(StringLocalizer["Ascending order"]);

    private string GetFieldStyle(FormElementField field)
    {
        string style = string.Empty;
        switch (field.Component)
        {
            case FormComponent.ComboBox:
            {
                if (field.DataItem is { ShowImageLegend: true, ReplaceTextOnGrid: false })
                {
                    style = "text-align:center";
                }

                break;
            }
            case FormComponent.CheckBox:
                style = "text-align:center;width:60px;";
                break;
            case FormComponent.Cnpj:
                style = "min-width:130px;";
                break;
            default:
            {
                if (field.DataType is FieldType.Float or FieldType.Int)
                {
                    if (!field.IsPk)
                        style = "text-align:right;";
                }

                break;
            }
        }

        return style;
    }

    private HtmlBuilder GetMultSelectThHtmlElement()
    {
        var th = new HtmlBuilder(HtmlTag.Th);

        bool hasPages = true;
        if (!GridView.IsPaggingEnabled())
        {
            hasPages = false;
        }
        else
        {
            int totalPages = (int)Math.Ceiling(GridView.TotalRecords / (double)GridView.CurrentSettings.TotalPerPage);
            if (totalPages <= 1)
                hasPages = false;
        }

        th.WithCssClass("jjselect")
            .Append(HtmlTag.Input, input =>
            {
                input.WithAttribute("type", "checkbox")
                    .WithNameAndId("jjchk_all")
                    .WithCssClass("form-check-input")
                    .WithToolTip(StringLocalizer["Mark|Unmark all from page"])
                    .WithAttribute("onclick",
                        "$('td.jjselect input').not(':disabled').prop('checked',$('#jjchk_all').is(':checked')).change();");
            });

        th.AppendIf(hasPages, HtmlTag.Span, span =>
        {
            span.WithCssClass("dropdown");
            span.Append(HtmlTag.A, a =>
            {
                a.WithAttribute("href", "#");
                a.WithAttribute(BootstrapHelper.DataToggle, "dropdown");
                a.WithCssClass("dropdown-toggle");
                a.AppendIf(BootstrapHelper.Version == 3,
                    new JJIcon("fa fa-caret-down fa-fw fa-lg").GetHtmlBuilder);
            });

            span.Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu");
                ul.Append(HtmlTag.Li, li =>
                {
                    li.WithCssClass("dropdown-item");
                    li.Append(HtmlTag.A, a =>
                    {
                        a.WithAttribute("href", "javascript:void(0);");
                        a.WithAttribute("onclick", $"FormView.unSelectAll('{GridView.Name}')");
                        a.AppendText(StringLocalizer["Unmark all selected records"]);
                    });
                });
                ul.AppendIf(GridView.TotalRecords <= 50000, HtmlTag.Li, li =>
                {
                    li.WithCssClass("dropdown-item");
                    li.Append(HtmlTag.A, a =>
                    {
                        a.WithAttribute("href", "javascript:void(0);");
                        a.WithAttribute("onclick", GridView.Scripts.GetSelectAllScript(GridView));
                        a.AppendText(GridView.StringLocalizer["Mark all {0} records", GridView.TotalRecords]);
                    });
                });
            });
        });

        return th;
    }
}