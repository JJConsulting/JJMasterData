using System.Globalization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJSlider : JJBaseControl
{
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public int? Value { get; set; }
    public bool ShowInput { get; set; } = true;

    public JJSlider(float minValue = 0, float maxValue = 100)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
    
    public static JJBaseControl GetInstance(FormElementField field, object value)
    {
        var slider = new JJSlider(field.MinValue ?? 0f, field.MaxValue ?? 100)
        {
            Name =  field.Name,
            Value = !string.IsNullOrEmpty(value?.ToString()) ? int.Parse(value.ToString()) : null
        };
        return slider;
    }

    internal override HtmlBuilder RenderHtml()
    {
        var html = new HtmlBuilder(HtmlTag.Div)
            .WithCssClass("row")
            .WithCssClass(CssClass)
            .AppendElement(HtmlTag.Div, col =>
            {
                col.WithCssClass(ShowInput ? "col-sm-9" : "col-sm-12");
                col.WithCssClassIf(BootstrapHelper.Version > 3, "d-flex justify-content-end align-items-center");
                col.AppendElement(GetHtmlSlider());
            });

        if (ShowInput)
        {
            var number = new JJTextBox
            {
                InputType = InputType.Number,
                Name = $"{Name}-value",
                MinValue = MinValue,
                Text = Value.ToString(),
                MaxValue = MaxValue,
                CssClass = "jjslider-value"
            };

            html.AppendElement(HtmlTag.Div, row =>
            {
                row.WithCssClass("col-sm-3");
                row.AppendElement(number);
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
           .WithAttribute("min", MinValue.ToString(CultureInfo.CurrentCulture))
           .WithAttribute("max", MaxValue.ToString(CultureInfo.CurrentCulture))
           .WithAttribute("step", "1")
           .WithAttributeIf(Value.HasValue, "value", Value?.ToString());

        return slider;
    }

}