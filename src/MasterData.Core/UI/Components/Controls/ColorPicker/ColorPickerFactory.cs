using JJMasterData.Core.DataDictionary.Models;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.UI.Components.ColorPicker;

internal sealed class ColorPickerFactory(IHttpContextAccessor formValues) : IControlFactory<JJColorPicker>
{
    public JJColorPicker Create()
    {
        return new JJColorPicker(formValues);
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