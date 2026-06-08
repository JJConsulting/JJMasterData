using JJMasterData.Core.DataDictionary.Models;
using Microsoft.AspNetCore.Http;

namespace JJMasterData.Core.UI.Components;

internal sealed class TextBoxFactory(IHttpContextAccessor formValues)
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