using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class SliderFactory(IFormValues formValues, IControlFactory<JJTextBox> textBoxFactory)
    : IControlFactory<JJSlider>
{
    private IFormValues FormValues { get; } = formValues;
    private IControlFactory<JJTextBox> TextBoxFactory { get; } = textBoxFactory;


    public JJSlider Create()
    {
        return new JJSlider(FormValues,TextBoxFactory);
    }

    public JJSlider Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var slider = Create();
        slider.Name = field.Name;
        slider.NumberOfDecimalPlaces = field.NumberOfDecimalPlaces;
        double minValue, maxValue, step;
        if (field.Attributes.TryGetValue(FormElementField.MinValueAttribute, out var tempValue))
            minValue = (double)tempValue;
        else
            minValue = 0f;

        if (field.Attributes.TryGetValue(FormElementField.MaxValueAttribute, out tempValue))
            maxValue = (double)tempValue;
        else
            maxValue = 100f;

        if (field.Attributes.TryGetValue(FormElementField.StepAttribute, out tempValue))
            step = (double)tempValue;
        else
            step = 1f;

        slider.MinValue = minValue;
        slider.MaxValue = maxValue;
        slider.Step = step;
        slider.Value = !string.IsNullOrEmpty(context.Value?.ToString()) ? double.Parse(context.Value.ToString()!) : null;
        return slider;
    }
}