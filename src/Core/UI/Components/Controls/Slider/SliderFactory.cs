using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

public class SliderFactory
{
    private IHttpContext HttpContext { get; }

    public SliderFactory(IHttpContext httpContext)
    {
        HttpContext = httpContext;
    }
    
    public JJSlider CreateSlider(FormElementField field, object value)
    {
        var slider = new JJSlider(HttpContext)
        {
            Name =  field.Name,
            NumberOfDecimalPlaces = field.NumberOfDecimalPlaces,
            MinValue = field.Attributes[FormElementField.MinValueAttribute] ?? 0f,
            MaxValue = field.Attributes[FormElementField.MaxValueAttribute] ?? 100,
            Step = (double)field.Attributes![FormElementField.StepAttribute],
            Value = !string.IsNullOrEmpty(value?.ToString()) ? double.Parse(value.ToString()) : null
        };
        return slider;
    }
}