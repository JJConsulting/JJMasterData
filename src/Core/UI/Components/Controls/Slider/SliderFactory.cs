using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.UI.Components;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.Web.Factories;

internal class SliderFactory : IControlFactory<JJSlider>
{
    private IFormValues FormValues { get; }
    private IComponentFactory<JJTextBox> TextBoxFactory { get; }


    public SliderFactory(IFormValues formValues, IComponentFactory<JJTextBox> textBoxFactory)
    {
        FormValues = formValues;
        TextBoxFactory = textBoxFactory;
    }
    
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