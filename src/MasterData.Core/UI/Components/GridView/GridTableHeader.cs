using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

internal sealed class GridTableHeader(JJGridView gridView)
{
    private readonly IStringLocalizer<MasterDataResources> _stringLocalizer  = gridView.StringLocalizer;

    public async Task<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = new HtmlBuilder(HtmlTag.Thead);
        if (gridView.DataSource?.Count == 0 && !gridView.ShowHeaderWhenEmpty)
            return html;

        var visibleFieldsThList = await GetVisibleFieldsThList();

        var tr = new HtmlBuilder(HtmlTag.Tr);
        
        tr.AppendIf(gridView.EnableMultiSelect, GetMultSelectThHtmlElement);
        tr.AppendRange(visibleFieldsThList);
        tr.AppendRange(GetActionsThList());
        
        html.Append(tr);

        return html;
    }

    private IEnumerable<HtmlBuilder> GetActionsThList()
    {
        var basicActions = gridView.TableActions.OrderBy(x => x.Order).ToList();
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

    private async Task<List<HtmlBuilder>> GetVisibleFieldsThList()
    {
        var thList = new List<HtmlBuilder>();
        var hasIcon = false;
        
        foreach (var field in await gridView.GetVisibleFieldsAsync())
        {
            var th = new HtmlBuilder(HtmlTag.Th);
            var style = GetThStyle(field);

            th.WithAttributeIfNotEmpty("style", style);
            th.Append(HtmlTag.Span, span =>
            {
                if (gridView.EnableSorting && field.DataBehavior is FieldBehavior.Real)
                {
                    span.WithCssClass("jjenable-sorting");
                    span.WithOnClick(gridView.Scripts.GetSortingScript(field.Name));
                }
                span.AppendSpan(span=>
                {
                    if (string.IsNullOrEmpty(field.Label)) 
                    {
                        span.AppendText(field.Name);
                    }
                    else
                    {
                        span.AppendText(_stringLocalizer[field.Label]);
                    }
                    
                    if (!string.IsNullOrEmpty(field.HelpDescription))
                    {
                        span.AppendSpan(circle =>
                        {
                            circle.WithCssClass("fa fa-question-circle help-description");
                            circle.WithToolTip(_stringLocalizer[field.HelpDescription!]);
                        });
                    }
                });
            });

            var orderByString = gridView.CurrentOrder.ToQueryParameter();
            if (!string.IsNullOrEmpty(orderByString))
            {
                foreach (var orderField in orderByString.Split(','))
                {
                    var order = orderField.Trim();
                    if (string.IsNullOrWhiteSpace(order))
                        break;

                    if (order.StartsWith("["))
                    {
                        order = order.Replace("[", "");
                        order = order.Replace("]", "");
                    }

                    if (order.Equals($"{field.Name} DESC"))
                    {
                        th.Append(GetDescendingIcon());
                        hasIcon = true;
                    }
                    else if (order.Equals($"{field.Name} ASC") || order.Equals(field.Name))
                    {
                        th.Append(GetAscendingIcon());
                        hasIcon = true;
                    }
                }
            }
            else
            {
                if (field.Name.EndsWith("::DESC"))
                {
                    th.Append(GetDescendingIcon());
                    hasIcon = true;
                }
                else if (field.Name.EndsWith("::ASC"))
                {
                    th.Append(GetAscendingIcon());
                    hasIcon = true;
                }
            }

            var currentFilter = await gridView.GetCurrentFilterAsync();

            if (IsAppliedFilter(field, currentFilter))
            {
                hasIcon = true;
                th.AppendComponent(new JJIcon("fa fa-filter text-info")
                {
                    Tooltip = _stringLocalizer["Applied Filter"]
                });
            }

            if (hasIcon)
            {
                th.WithCssClass("text-nowrap");
            }
            
            thList.Add(th);
        }

        return thList;
    }

    private static bool IsAppliedFilter(ElementField field, Dictionary<string, object> currentFilter)
    {
        if (field.Filter.Type is FilterMode.None)
            return false;
        
        var hasFieldOrFromKey = currentFilter.ContainsKey(field.Name) || currentFilter.ContainsKey($"{field.Name}_from");

        return hasFieldOrFromKey;
    }

    private HtmlBuilder GetAscendingIcon() => new JJIcon("fa fa-sort-amount-asc text-info").GetHtmlBuilder()
        .WithToolTip(_stringLocalizer["Ascending order"]);

    private HtmlBuilder GetDescendingIcon() => new JJIcon("fa fa-sort-amount-desc text-info").GetHtmlBuilder()
        .WithToolTip(_stringLocalizer["Descending order"]);

    private static string GetThStyle(FormElementField field)
    {
        switch (field.GridAlignment)
        {
            case GridAlignment.Left:
                return "text-align:left";
            case GridAlignment.Center:
                return "text-align:center";
            case GridAlignment.Right:
                return "text-align:right";
        }
        switch (field.Component)
        {
            case FormComponent.ComboBox or FormComponent.RadioButtonGroup:
            {
                if (field.DataItem is { 
                        ShowIcon: true,
                        GridBehavior: DataItemGridBehavior.Icon or DataItemGridBehavior.IconWithDescription 
                    })
                {
                    return "text-align:center";
                }

                break;
            }
            case FormComponent.CheckBox:
                return "text-align:center;";
            case FormComponent.Icon:
                return "text-align:center;";
            default:
            {
                if (field.DataType is FieldType.Float or FieldType.Int or FieldType.Decimal)
                {
                    if (!field.IsPk)
                        return "text-align:right;";
                }

                break;
            }
        }

        return string.Empty;
    }

    private HtmlBuilder GetMultSelectThHtmlElement()
    {
        var th = new HtmlBuilder(HtmlTag.Th);

        var hasPages = true;
        if (!gridView.IsPagingEnabled())
        {
            hasPages = false;
        }
        else
        {
            if (gridView.TotalOfPages <= 1)
                hasPages = false;
        }

        th.WithCssClass("jj-checkbox")
            .Append(HtmlTag.Input, input =>
            {
                input.WithAttribute("type", "checkbox")
                    .WithNameAndId($"{gridView.Name}-checkbox-select-all-rows")
                    .WithCssClass("form-check-input")
                    .WithToolTip(_stringLocalizer["Mark|Unmark all from page"])
                    .WithOnClick(
                        "GridViewSelectionHelper.selectAllAtSamePage(this)");
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
                    (Func<HtmlBuilder>)new JJIcon("fa fa-caret-down fa-fw fa-lg").GetHtmlBuilder);
            });

            span.Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu");
                ul.Append(HtmlTag.Li, li =>
                {
                    li.Append(HtmlTag.A, a =>
                    {
                        a.WithCssClass("dropdown-item");
                        a.WithAttribute("href", "javascript:void(0);");
                        a.WithOnClick( $"GridViewSelectionHelper.unSelectAll('{gridView.Name}')");
                        a.AppendText(_stringLocalizer["Unmark all selected records"]);
                    });
                });
                ul.AppendIf(gridView.TotalOfRecords <= 50000, HtmlTag.Li, li =>
                {
                    li.Append(HtmlTag.A, a =>
                    {
                        a.WithCssClass("dropdown-item");
                        a.WithAttribute("href", "javascript:void(0);");
                        a.WithOnClick( gridView.Scripts.GetSelectAllScript());
                        a.AppendText(gridView.StringLocalizer["Mark all {0} records", gridView.TotalOfRecords]);
                    });
                });
            });
        });

        return th;
    }
}