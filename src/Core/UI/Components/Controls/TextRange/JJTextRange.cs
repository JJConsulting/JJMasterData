using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
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

    protected override ValueTask<ComponentResult> BuildResultAsync()
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
        
        return new(new RenderedComponentResult(div));
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
            .AppendText($"{StringLocalizer["Periods"]}&nbsp;")
            .Append(HtmlTag.Span, span => { span.WithCssClass("caret"); });
    }

    private string GetClearScript()
    {
        return GetjQueryFromToScript(string.Empty, string.Empty, false);
    }

    private string GetLastThreeMonthsScript(DateTime now)
    {
        var lastQuarter = now.AddMonths(-3);
        var dtLastQuarterFrom = new DateTime(lastQuarter.Year, lastQuarter.Month, 1).ToShortDateString();
        var dtMonthTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))
            .ToShortDateString();

        return GetjQueryFromToScript(dtLastQuarterFrom, dtMonthTo, IsTimeAware);
    }

    private string GetLastMonthScript(DateTime now)
    {
        var lastMonth = now.AddMonths(-1);
        var dtLastMonthFrom = new DateTime(lastMonth.Year, lastMonth.Month, 1).ToShortDateString();
        var dtLastMonthTo = new DateTime(lastMonth.Year, lastMonth.Month,
            DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month)).ToShortDateString();

        return GetjQueryFromToScript(dtLastMonthFrom, dtLastMonthTo, IsTimeAware);
    }

    private string GetThisMonthScript(DateTime now)
    {
        var dtMonthFrom = new DateTime(now.Year, now.Month, 1).ToShortDateString();
        var dtMonthTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))
            .ToShortDateString();
        return GetjQueryFromToScript(dtMonthFrom, dtMonthTo, IsTimeAware);
    }

    private string GetYesterdayScript(DateTime now)
    {
        string yesterdayDate = now.AddDays(-1).ToShortDateString();
        return GetjQueryFromToScript(yesterdayDate, yesterdayDate, IsTimeAware);
    }

    private string GetTodayScript(DateTime now)
    {
        return GetjQueryFromToScript(now.ToShortDateString(), now.ToShortDateString(), IsTimeAware);
    }

    private static HtmlBuilder GetListItem(string label, string script)
    {
        return new HtmlBuilder(HtmlTag.Li)
            .WithOnClick( script)
            .AppendDiv(div =>
            {
                div.WithCssClass("dropdown-item");
                div.AppendText(label);
            });
    }


    private string GetjQueryFromToScript(string valueFrom, string valueTo, bool isTimeAware)
    {
        string timeAwareTo = isTimeAware ? " 23:59" : string.Empty;
        string timeAwareFrom = isTimeAware ? " 00:00" : string.Empty;
        return $"$('#{Name}_from').val('{valueFrom}{timeAwareFrom}');$('#{Name}_to').val('{valueTo}{timeAwareTo}')";
    }
}