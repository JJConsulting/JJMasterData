using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Commons.Localization;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.TextRange;

public class JJTextRange : ControlBase
{
    private IControlFactory<JJTextGroup> TextBoxGroupFactory { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    internal ControlBase FromField { get; set; }
    internal ControlBase ToField { get; set; }

    public FieldType FieldType { get; set; }
    private bool EnableDatePeriods => FieldType is FieldType.Date or FieldType.DateTime or FieldType.DateTime2;
    private bool IsTimeAware => FieldType is FieldType.DateTime or FieldType.DateTime2;
    
    public bool IsVerticalLayout { get; set; }
    
    public JJTextRange(IFormValues formValues, IControlFactory<JJTextGroup> textBoxGroupFactory,IStringLocalizer<JJMasterDataResources> stringLocalizer) : base(formValues)
    {
        TextBoxGroupFactory = textBoxGroupFactory;
        StringLocalizer = stringLocalizer;
    }

    protected override async Task<ComponentResult> BuildResultAsync()
    {
        var div = new HtmlBuilder(string.Empty);
        div.WithCssClass(CssClass);
        div.WithAttributes(Attributes);
        await div.AppendAsync(HtmlTag.Div, async div =>
        {
            div.WithCssClass(IsVerticalLayout ? "col-sm-5" : "col-sm-3");

            FromField.Name = $"{Name}_from";
            FromField.Enabled = Enabled;

            await div.AppendControlAsync(FromField);
        });
        await div.AppendAsync(HtmlTag.Div, async div =>
        {
            div.WithCssClass(IsVerticalLayout ? "col-sm-5" : "col-sm-3");

            ToField.Name = $"{Name}_to";
            ToField.Enabled = Enabled;

            await div.AppendControlAsync(ToField);
        });
        div.Append(HtmlTag.Div, div =>
        {
            div.WithCssClass(IsVerticalLayout ? "col-sm-2" : "col-sm-4");
            div.AppendIf(EnableDatePeriods, GetDatePeriodsHtmlElement);
        });

        var result = new RenderedComponentResult(div);

        return result;
    }

    private HtmlBuilder GetDatePeriodsHtmlElement()
    {
        var now = DateTime.Today;

        var dropdown = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("dropdown")
            .Append(GetDropdownButton())
            .Append(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu");
                ul.WithAttribute("aria-labelledby", $"dropdown_{Name}");
                ul.Append(GetListItem(StringLocalizer["Today"], GetTodayScript(now)));
                ul.Append(GetListItem(StringLocalizer["Yesterday"], GetYesterdayScript(now)));
                ul.Append(GetListItem(StringLocalizer["This month"], GetThisMonthScript(now)));
                ul.Append(GetListItem(StringLocalizer["Last month"], GetLastMonthScript(now)));
                ul.Append(GetListItem(StringLocalizer["Last three months"], GetLastThreeMonthsScript(now)));
                ul.Append(GetListItem(StringLocalizer["Clear"], GetClearScript()));
            });

        return dropdown;
    }

    private HtmlBuilder GetDropdownButton()
    {
        return new HtmlBuilder(HtmlTag.Button)
            .WithAttribute("type", "button")
            .WithAttribute("id", $"dropdown_{Name}")
            .WithCssClass($"dropdown-toggle{BootstrapHelper.DefaultButton}")
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
            .WithAttribute("onclick", script)
            .Append(HtmlTag.A, a =>
            {
                a.WithCssClass("dropdown-item");
                a.WithAttribute("href", "#");
                a.AppendText(label);
            });
    }


    private string GetjQueryFromToScript(string valueFrom, string valueTo, bool isTimeAware)
    {
        string timeAwareTo = isTimeAware ? " 23:59" : string.Empty;
        string timeAwareFrom = isTimeAware ? " 00:00" : string.Empty;
        return $"$('#{Name}_from').val('{valueFrom}{timeAwareFrom}');$('#{Name}_to').val('{valueTo}{timeAwareTo}')";
    }
}