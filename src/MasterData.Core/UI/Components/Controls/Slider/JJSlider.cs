using System.Globalization;
using System.Threading.Tasks;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.UI.Components;

public class JJSlider(IFormValues formValues, IControlFactory<JJTextBox> textBoxFactory)
    : ControlBase(formValues)
{
    private IControlFactory<JJTextBox> TextBoxFactory { get; } = textBoxFactory;
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double? Value { get; set; }
    public double Step { get; set; } = 1;
    public bool ShowInput { get; set; } = true;
    public int NumberOfDecimalPlaces { get; set; }

    protected override ValueTask<ComponentResult> BuildResultAsync()
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
            var number = TextBoxFactory.Create();
            number.InputType = InputType.Number;
            number.Name = $"{Name}-value";
            number.MinValue = MinValue;
            number.Enabled = Enabled;
            number.Text = Value.ToString();
            number.NumberOfDecimalPlaces = NumberOfDecimalPlaces;
            number.MaxValue = MaxValue;
            number.CssClass = "jjslider-value";
            number.Attributes["step"] = Step.ToString();

            html.Append(HtmlTag.Div, row =>
            {
                row.WithCssClass("col-sm-3");
                row.Append(number.GetHtmlBuilder());
            });
        }
        
        var result = new RenderedComponentResult(html);
        
        return new ValueTask<ComponentResult>(result);
    }
    
    private HtmlBuilder GetHtmlSlider()
    {
        var slider = new HtmlBuilder(HtmlTag.Input)
           .WithAttributes(Attributes)
           .WithAttribute("type", "range")
           .WithNameAndId(Name)
           .WithCssClass("jjslider form-range")
           .WithAttributeIf(!Enabled,"disabled")
           .WithAttributeIf(NumberOfDecimalPlaces > 0 , "jj-decimal-places", NumberOfDecimalPlaces.ToString())
           .WithAttribute("min", MinValue.ToString(CultureInfo.InvariantCulture))
           .WithAttribute("max", MaxValue.ToString(CultureInfo.InvariantCulture))
           .WithAttribute("step", Step.ToString(CultureInfo.InvariantCulture))
           .WithAttributeIf(Value.HasValue, "value", Value?.ToString());

        return slider;
    }

}