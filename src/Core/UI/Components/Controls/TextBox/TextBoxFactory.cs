using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class TextBoxFactory(IFormValues formValues, IEncryptionService encryptionService)
    : IControlFactory<JJTextBox>
{
    private IFormValues FormValues { get; } = formValues;
    private IEncryptionService EncryptionService { get; } = encryptionService;

    public JJTextBox Create()
    {
        return new JJTextBox(FormValues);
    }

    public JJTextBox Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        return new JJTextBox(FormValues)
        {
            Name = field.Name,
            Text = context.Value?.ToString()
        };
    }
}