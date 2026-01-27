using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJConsulting.Html;
using JJConsulting.Html.Extensions;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.Extensions;
using JJMasterData.Core.Http.Abstractions;

using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.TextRange;

public class JJTextRange(IFormValues formValues,
        IStringLocalizer<MasterDataResources> stringLocalizer)
    : ControlBase(formValues)
{
    private IStringLocalizer<MasterDataResources> StringLocalizer { get; } = stringLocalizer;

    internal JJTextGroup FromField { get; set; }
    internal JJTextGroup ToField { get; set; }

    public FieldType FieldType { get; set; }
    private bool EnableDatePeriods => FieldType is FieldType.Date or FieldType.DateTime or FieldType.DateTime2;
    private bool IsTimeAware => FieldType is FieldType.DateTime or FieldType.DateTime2;
    
    public bool IsVerticalLayout { get; set; }

    protected internal override ValueTask<HtmlBuilder> GetHtmlBuilderAsync()
    {
        var html = GetHtmlBuilder();

        return html.AsValueTask();
    }

    private HtmlBuilder GetHtmlBuilder()
    {
        var div = new HtmlBuilder(HtmlTag.Div);
        div.WithCssClass("row");
        div.WithCssClass(CssClass);
        div.WithAttributes(Attributes);
        
        FromField.Name = $"{Name}_from";
        FromField.Enabled = Enabled;

        var fromFieldHtml = FromField.GetHtmlBuilder();
        
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-5");
            div.Append(fromFieldHtml);
        });
        
        ToField.Name = $"{Name}_to";
        ToField.Enabled = Enabled;

        var toFieldHtml = ToField.GetHtmlBuilder();

        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-5");
            div.Append(toFieldHtml);
        });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-2");
            div.WithCssClassIf(IsVerticalLayout, "text-end");
            div.AppendIf(EnableDatePeriods, GetDatePeriodsHtmlElement);
        });
        return div;
    }

    private HtmlBuilder GetDatePeriodsHtmlElement()
    {


        var dropdown = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("dropdown")
            .Append(GetDropdownButton())
            .Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu");
                ul.WithAttribute("aria-labelledby", $"dropdown_{Name}");
                ul.AppendRange(GetListItems());
            });

        return dropdown;
    }
    
    private IEnumerable<HtmlBuilder> GetListItems()
    {
        var now = DateTime.Today;
        
        yield return GetListItem(StringLocalizer["Today"], GetTodayScript(now));
        yield return GetListItem(StringLocalizer["Yesterday"], GetYesterdayScript(now));
        yield return GetListItem(StringLocalizer["This week"], GetThisWeekScript(now));
        yield return GetListItem(StringLocalizer["This month"], GetThisMonthScript(now));
        yield return GetListItem(StringLocalizer["Last month"], GetLastMonthScript(now));
        yield return GetListItem(StringLocalizer["Last three months"], GetLastThreeMonthsScript(now));
        yield return GetListItem(StringLocalizer["Clear"], GetClearScript());
    }

    private HtmlBuilder GetDropdownButton()
    {
        return new HtmlBuilder(HtmlTag.Button)
            .WithAttribute("type", "button")
            .WithAttribute("id", $"dropdown_{Name}")
            .WithCssClass($"dropdown-toggle {BootstrapHelper.BtnDefault}")
            .WithAttribute("aria-haspopup", "true")
            .WithAttribute("aria-expanded", "true")
            .WithAttribute(BootstrapHelper.DataToggle, "dropdown")
            .AppendText($"{StringLocalizer["Periods"]} ")
            .Append(HtmlTag.Span, span => span.WithCssClass("caret"));
    }

    private string GetClearScript()
    {
        return GetjQueryFromToScript(string.Empty, string.Empty, false);
    }

    private string GetLastThreeMonthsScript(DateTime date)
    {
        var lastQuarter = date.AddMonths(-3);
        var dtLastQuarterFrom = new DateTime(lastQuarter.Year, lastQuarter.Month, 1).ToShortDateString();
        var dtMonthTo = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month))
            .ToShortDateString();

        return GetjQueryFromToScript(dtLastQuarterFrom, dtMonthTo, IsTimeAware);
    }

    private string GetLastMonthScript(DateTime date)
    {
        var lastMonth = date.AddMonths(-1);
        var dtLastMonthFrom = new DateTime(lastMonth.Year, lastMonth.Month, 1).ToShortDateString();
        var dtLastMonthTo = new DateTime(lastMonth.Year, lastMonth.Month,
            DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month)).ToShortDateString();

        return GetjQueryFromToScript(dtLastMonthFrom, dtLastMonthTo, IsTimeAware);
    }

    private string GetThisMonthScript(DateTime date)
    {
        var dtMonthFrom = new DateTime(date.Year, date.Month, 1).ToShortDateString();
        var dtMonthTo = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month))
            .ToShortDateString();
        return GetjQueryFromToScript(dtMonthFrom, dtMonthTo, IsTimeAware);
    }

    private string GetThisWeekScript(DateTime date)
    {
        var diff = (int)date.DayOfWeek - (int)DayOfWeek.Monday;
        if (diff < 0) 
            diff += 7;
        
        var weekStart = date.AddDays(-diff).Date.ToShortDateString();
        var weekEnd = date.AddDays(6 - diff).Date.ToShortDateString();

        return GetjQueryFromToScript(weekStart, weekEnd, IsTimeAware);
    }
    
    private string GetYesterdayScript(DateTime date)
    {
        string yesterdayDate = date.AddDays(-1).ToShortDateString();
        return GetjQueryFromToScript(yesterdayDate, yesterdayDate, IsTimeAware);
    }

    private string GetTodayScript(DateTime date)
    {
        var today = date.ToShortDateString();
        return GetjQueryFromToScript(today, today, IsTimeAware);
    }

    private static HtmlBuilder GetListItem(string label, string script)
    {
        return new HtmlBuilder(HtmlTag.Li)
            .WithOnClick(script)
            .Append(BootstrapHelper.Version == 5 ? HtmlTag.Div : HtmlTag.A,tag =>
            {
                tag.WithAttributeIf(BootstrapHelper.Version == 3,"href","javascript:void(0)");
                tag.WithCssClass("dropdown-item");
                tag.AppendText(label);
            });
    }


    private string GetjQueryFromToScript(string valueFrom, string valueTo, bool isTimeAware)
    {
        string timeAwareTo = isTimeAware ? " 23:59" : string.Empty;
        string timeAwareFrom = isTimeAware ? " 00:00" : string.Empty;
        return $"$('#{Name}_from').val('{valueFrom}{timeAwareFrom}');$('#{Name}_to').val('{valueTo}{timeAwareTo}')";
    }
}