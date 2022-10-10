using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Html;

namespace JJMasterData.Core.WebComponents;

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

    internal override HtmlElement GetHtmlElement()
    {
        var html = new HtmlElement(HtmlTag.Div)
            .WithCssClass(CssClass)
            .AppendElement(HtmlTag.Div, row =>
            {
                row.WithCssClass(ShowInput ? "col-sm-9" : "col-sm-12");
                row.AppendElement(GetHtmlSlider());
            });

        if (ShowInput)
        {
            var number = new JJTextBox
            {
                InputType = InputType.Number,
                Name = $"{Name}-value",
                MinValue = MinValue,
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

    private HtmlElement GetHtmlSlider()
    {
        var slider = new HtmlElement(HtmlTag.Input)
           .WithAttributes(Attributes)
           .WithAttribute("type", "range")
           .WithNameAndId(Name)
           .WithCssClass("jjslider form-range")
           .WithAttribute("min", MinValue.ToString())
           .WithAttribute("max", MaxValue.ToString())
           .WithAttribute("step", "1")
           .WithAttributeIf(Value.HasValue, "value", Value?.ToString());

        return slider;
    }

}