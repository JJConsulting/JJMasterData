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
        slider.MinValue = (double)(field.Attributes[FormElementField.MinValueAttribute] ?? 0f);
        slider.MaxValue = (double)(field.Attributes[FormElementField.MaxValueAttribute] ?? 100);
        slider.Step = (double)field.Attributes![FormElementField.StepAttribute];
        slider.Value = !string.IsNullOrEmpty(context.Value?.ToString()) ? double.Parse(context.Value.ToString()!) : null;
        return slider;
    }
}