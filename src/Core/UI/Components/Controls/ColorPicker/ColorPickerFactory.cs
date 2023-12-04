using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.ColorPicker;

internal class ColorPickerFactory(IFormValues formValues) : IControlFactory<JJColorPicker>
{
    private IFormValues FormValues { get; } = formValues;

    public JJColorPicker Create()
    {
        return new JJColorPicker(FormValues);
    }

    public JJColorPicker Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        var colorPicker = Create();
        colorPicker.Name = field.Name;
        colorPicker.Title = field.LabelOrName;
        colorPicker.Text = context.Value?.ToString();

        return colorPicker;
    }
}