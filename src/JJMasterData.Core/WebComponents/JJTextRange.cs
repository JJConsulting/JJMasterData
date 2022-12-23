using System;
using System.Collections;
using JJMasterData.Commons.Dao.Entity;
using JJMasterData.Commons.Language;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.WebComponents.Factories;

namespace JJMasterData.Core.WebComponents;

public class JJTextRange : JJBaseControl
{
    private JJBaseControl FromField { get; set; }
    private JJBaseControl ToField { get; set; }

    public FieldType FieldType { get; set; }
    private bool EnableDatePeriods => FieldType is FieldType.Date or FieldType.DateTime or FieldType.DateTime2;
    private bool IsTimeAware => FieldType is FieldType.DateTime or FieldType.DateTime2;


    public JJTextRange(IHttpContext httpContext) : base(httpContext)
    {
        
    }

    internal static JJBaseControl GetInstance(FormElementField field, Hashtable values, IHttpContext httpContext)
    {
        string valueFrom = "";
        if (values != null && values.Contains(field.Name + "_from"))
        {
            valueFrom = values[field.Name + "_from"].ToString();
        }
        
        var range = new JJTextRange(httpContext);
        range.FieldType = field.DataType;
        range.FromField = new TextGroupFactory(httpContext).CreateTextGroup(field);
        range.FromField.Text = valueFrom;
        range.FromField.Name = field.Name + "_from";
        range.FromField.PlaceHolder = Translate.Key("From");
        
        string valueTo = "";
        if (values != null && values.Contains(field.Name + "_to"))
        {
            valueTo = values[field.Name + "_to"].ToString();
        }
        
        range.ToField = new TextGroupFactory(httpContext).CreateTextGroup(field);
        range.ToField.Text = valueTo;
        range.ToField.Name = field.Name + "_to";
        range.ToField.PlaceHolder = Translate.Key("To");

        return range;
    }

    internal override HtmlBuilder RenderHtml()
    {
        var div = new HtmlBuilder(string.Empty);
        div.WithCssClass(CssClass);
        div.WithAttributes(Attributes);
        div.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-3");
            
            FromField.Name = Name + "_from";
            FromField.Enabled = Enabled;
            
            div.AppendElement(FromField);
        });
        div.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-3");
            
            ToField.Name = Name + "_to";
            ToField.Enabled = Enabled;
            
            div.AppendElement(ToField);
        });
        div.AppendElement(HtmlTag.Div, div =>
        {
            div.WithCssClass("col-sm-4");
            div.AppendElementIf(EnableDatePeriods, GetDatePeriodsHtmlElement);
        });

        return div;
    }

    private HtmlBuilder GetDatePeriodsHtmlElement()
    {
        var now = DateTime.Today;

        var dropdown = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("dropdown")
            .AppendElement(GetDropdownButton())
            .AppendElement(HtmlTag.Ul, ul =>
            {
                ul.WithCssClass("dropdown-menu");
                ul.WithAttribute("aria-labelledby", $"dropdown_{Name}");
                ul.AppendElement(GetListItem(Translate.Key("Today"), GetTodayScript(now)));
                ul.AppendElement(GetListItem(Translate.Key("Yesterday"), GetYesterdayScript(now)));
                ul.AppendElement(GetListItem(Translate.Key("This month"), GetThisMonthScript(now)));
                ul.AppendElement(GetListItem(Translate.Key("Last month"), GetLastMonthScript(now)));
                ul.AppendElement(GetListItem(Translate.Key("Last three months"), GetLastThreeMonthsScript(now)));
                ul.AppendElement(GetListItem(Translate.Key("Clear"), GetClearScript()));
            });

        return dropdown;
    }
    
    private HtmlBuilder GetDropdownButton()
    {
        return new HtmlBuilder(HtmlTag.Button)
            .WithAttribute("type", "button")
            .WithAttribute("id", $"dropdown_{Name}")
            .WithCssClass("dropdown-toggle" + BootstrapHelper.DefaultButton)
            .WithAttribute("aria-haspopup", "true")
            .WithAttribute("aria-expanded", "true")
            .WithAttribute(BootstrapHelper.DataToggle, "dropdown")
            .AppendText(Translate.Key("Periods") + "&nbsp;")
            .AppendElement(HtmlTag.Span, span => { span.WithCssClass("caret"); });
    }

    private string GetClearScript()
    {
        return GetjQueryFromToScript(string.Empty, string.Empty, false);
    }

    private string GetLastThreeMonthsScript(DateTime now)
    {
        var lastQuarter =now.AddMonths(-3);
        var dtLastQuarterFrom = new DateTime(lastQuarter.Year, lastQuarter.Month, 1).ToShortDateString();
        var dtMonthTo = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month)).ToShortDateString();
        
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

    private HtmlBuilder GetListItem(string label, string script)
    {
        return new HtmlBuilder(HtmlTag.Li)
            .WithCssClass("dropdown-item")
            .AppendElement(HtmlTag.A, a =>
            {
                a.WithAttribute("href", "#");
                a.WithAttribute("onclick", script);
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