using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal sealed class TextBoxFactory(IFormValues formValues)
    : IControlFactory<JJTextBox>
{
    public JJTextBox Create()
    {
        return new JJTextBox(formValues);
    }

    public JJTextBox Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        return new JJTextBox(formValues)
        {
            Name = field.Name,
            Text = context.Value?.ToString()
        };
    }
}