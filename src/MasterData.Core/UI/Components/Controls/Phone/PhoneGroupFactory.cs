using System;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Phone;

internal sealed class PhoneGroupFactory(IFormValues formValues)
    : IControlFactory<JJPhoneGroup>
{
    public JJPhoneGroup Create()
    {
        return new JJPhoneGroup(formValues);
    }

    public JJPhoneGroup Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var phoneGroup = Create();
        phoneGroup.SetAttributes(field.Attributes);
        phoneGroup.Name = field.Name;
        phoneGroup.Text = context.Value?.ToString() ?? string.Empty;
        phoneGroup.MaxLength = field.Size > 0 ? field.Size : 15;
        phoneGroup.InputType = InputType.Phone;

        return phoneGroup;
    }
}
