using System.Globalization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.Web.Components;

public class JJSlider : JJBaseControl
{
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public float? Value { get; set; }
    public double Step { get; set; } = 1;
    public bool ShowInput { get; set; } = true;
    public int NumberOfDecimalPlaces { get; set; }
    public JJSlider(float minValue = 0, float maxValue = 100)
    {
        MinValue = minValue;
        MaxValue = maxValue;
    }
    
    public static JJBaseControl GetInstance(FormElementField field, object value)
    {
        var slider = new JJSlider(field.Attributes[FormElementField.MinValueAttribute] ?? 0f, field.Attributes[FormElementField.MaxValueAttribute] ?? 100)
        {
            Name =  field.Name,
            NumberOfDecimalPlaces = field.NumberOfDecimalPlaces,
            Step = (double)field.Attributes![FormElementField.StepAttribute],
            Value = !string.IsNullOrEmpty(value?.ToString()) ? float.Parse(value.ToString()) : null
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
                Enabled = Enabled,
                Text = Value.ToString(),
                NumberOfDecimalPlaces = NumberOfDecimalPlaces,
                MaxValue = MaxValue,
                CssClass = "jjslider-value"
            };

            number.Attributes["step"] = Step;

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
           .WithAttributeIf(!Enabled,"disabled")
           .WithAttributeIf(NumberOfDecimalPlaces > 0 , "jjdecimalplaces", NumberOfDecimalPlaces.ToString())
           .WithAttribute("min", MinValue.ToString(CultureInfo.InvariantCulture))
           .WithAttribute("max", MaxValue.ToString(CultureInfo.InvariantCulture))
           .WithAttribute("step", Step.ToString(CultureInfo.InvariantCulture))
           .WithAttributeIf(Value.HasValue, "value", Value?.ToString());

        return slider;
    }

}