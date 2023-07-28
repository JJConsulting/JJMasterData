using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

internal class SliderFactory : IControlFactory<JJSlider>
{
    private IHttpContext HttpContext { get; }

    public SliderFactory(IHttpContext httpContext)
    {
        HttpContext = httpContext;
    }
    
    public JJSlider Create()
    {
        return new JJSlider(HttpContext);
    }

    public JJSlider Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var slider = new JJSlider(HttpContext)
        {
            Name =  field.Name,
            NumberOfDecimalPlaces = field.NumberOfDecimalPlaces,
            MinValue = field.Attributes[FormElementField.MinValueAttribute] ?? 0f,
            MaxValue = field.Attributes[FormElementField.MaxValueAttribute] ?? 100,
            Step = (double)field.Attributes![FormElementField.StepAttribute],
            Value = !string.IsNullOrEmpty(context.Value?.ToString()) ? double.Parse(context.Value.ToString()) : null
        };
        return slider;
    }
}