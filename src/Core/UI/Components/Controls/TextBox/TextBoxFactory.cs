using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Http.Abstractions;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Controls.TextBox;

public class TextBoxFactory : IComponentFactory<JJTextBox>
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
}