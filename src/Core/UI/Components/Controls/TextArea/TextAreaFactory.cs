using JJMasterData.Commons.Localization;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataManager;
using JJMasterData.Core.Web.Components;
using JJMasterData.Core.Web.Http.Abstractions;
using Microsoft.Extensions.Localization;
using System;
using System.Threading.Tasks;
using JJMasterData.Commons.Cryptography;

namespace JJMasterData.Core.Web.Factories;

internal class TextAreaFactory : IControlFactory<JJTextArea>
{
    private IHttpRequest HttpRequest { get; }
    private IEncryptionService EncryptionService { get; }
    private IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public TextAreaFactory(IHttpRequest httpRequest, IEncryptionService encryptionService,IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        HttpRequest = httpRequest;
        EncryptionService = encryptionService;
        StringLocalizer = stringLocalizer;
    }
    public JJTextArea Create()
    {
        return new JJTextArea(HttpRequest,EncryptionService, StringLocalizer);
    }

    public JJTextArea Create(FormElement formElement, FormElementField field, ControlContext context)
    {
        if (field == null)
            throw new ArgumentNullException(nameof(field));

        var text = Create();
        text.SetAttr(field.Attributes);
        text.ToolTip = field.HelpDescription;
        text.MaxLength = field.Size;
        text.Text = context.Value != null ? context.Value.ToString() : "";
        text.Name = field.Name;

        return text;
    }
}