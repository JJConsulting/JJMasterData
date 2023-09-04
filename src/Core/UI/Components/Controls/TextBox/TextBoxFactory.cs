using JJMasterData.Commons.Cryptography;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.Web;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Factories;
using JJMasterData.Core.Web.Http.Abstractions;

namespace JJMasterData.Core.UI.Components.Controls.TextBox;

public class TextBoxFactory : IComponentFactory<JJTextBox>
{
    private IHttpRequest HttpRequest { get; }
    private IEncryptionService EncryptionService { get; }

    public TextBoxFactory(IHttpRequest httpRequest, IEncryptionService encryptionService)
    {
        HttpRequest = httpRequest;
        EncryptionService = encryptionService;
    }
    public JJTextBox Create()
    {
        return new JJTextBox(HttpRequest);
    }
}