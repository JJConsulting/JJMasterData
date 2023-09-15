using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;

namespace JJMasterData.Core.UI.Components.Controls.TextBox;

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