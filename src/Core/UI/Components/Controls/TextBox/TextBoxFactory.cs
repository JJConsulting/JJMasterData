using JJMasterData.Commons.Security.Cryptography.Abstractions;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.Http.Abstractions;

namespace JJMasterData.Core.UI.Components;

internal class TextBoxFactory : IControlFactory<JJTextBox>
{
    private IFormValues FormValues { get; }
    private IEncryptionService EncryptionService { get; }

    public TextBoxFactory(IFormValues formValues, IEncryptionService encryptionService)
    {
        FormValues = formValues;
        EncryptionService = encryptionService;
    }
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