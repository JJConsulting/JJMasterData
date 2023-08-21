using System.Globalization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.UI.Components.Controls;
using JJMasterData.Core.Web.Html;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Components;

public class JJSlider : HtmlControl
{
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double? Value { get; set; }
    public double Step { get; set; } = 1;
    public bool ShowInput { get; set; } = true;
    public int NumberOfDecimalPlaces { get; set; }
    
    public JJSlider(IHttpContext httpContext) : base(httpContext)
    {

    }

    internal override HtmlBuilder BuildHtml()  
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row")
            .WithCssClass(CssClass)
            .Append(HtmlTag.Div, col =>
            {
                col.WithCssClass(ShowInput ? "col-sm-9" : "col-sm-12");
                col.WithCssClassIf(BootstrapHelper.Version > 3, "d-flex justify-content-end align-items-center");
                col.Append(GetHtmlSlider());
            });

        if (ShowInput)
        {
            var number = new JJTextBox(CurrentContext)
            {
                InputType = InputType.Number,
                Name = $"{Name}-value",
                MinValue = MinValue,
                Enabled = Enabled,
                Text = Value.ToString(),
                NumberOfDecimalPlaces = NumberOfDecimalPlaces,
                MaxValue = MaxValue,
                CssClass = "jjslider-value",
                Attributes =
                {
                    ["step"] = Step.ToString()
                }
            };

            html.Append(HtmlTag.Div, row =>
            {
                row.WithCssClass("col-sm-3");
                row.AppendComponent(number);
            });
        }

        return html;
    }
    
    private HtmlBuilder GetHtmlSlider()
    {
        var slider = new HtmlBuilder(HtmlTag.Input)
           .WithAttributes(Attributes)
           .WithAttribute("type", "range")
           .WithNameAndId(Name)
           .WithCssClass("jjslider form-range")
           .WithAttributeIf(!Enabled,"disabled")
           .WithAttributeIf(NumberOfDecimalPlaces > 0 , "jjdecimalplaces", NumberOfDecimalPlaces.ToString())
           .WithAttribute("min", MinValue.ToString(CultureInfo.InvariantCulture))
           .WithAttribute("max", MaxValue.ToString(CultureInfo.InvariantCulture))
           .WithAttribute("step", Step.ToString(CultureInfo.InvariantCulture))
           .WithAttributeIf(Value.HasValue, "value", Value?.ToString());

        return slider;
    }

}