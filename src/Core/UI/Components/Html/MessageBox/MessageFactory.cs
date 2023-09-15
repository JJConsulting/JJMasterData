using JJMasterData.Commons.Localization;
using JJMasterData.Core.UI.Components;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.Web.Components;

public class MessageFactory : IComponentFactory<JJMessageBox>
{
    public IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public MessageFactory(IStringLocalizer<JJMasterDataResources> stringLocalizer)
    {
        StringLocalizer = stringLocalizer;
    }
    
    public JJMessageBox Create()
    {
        return new JJMessageBox();
    }
    
    public JJMessageBox Create(string text, MessageIcon icon)
    {
        var messageBox = Create();
        messageBox.Text = text;
        messageBox.Icon = icon;
        messageBox.Size = MessageSize.Default;
        messageBox.Title = messageBox.Icon switch
        {
            MessageIcon.Error => StringLocalizer["Erro"],
            MessageIcon.Warning => StringLocalizer["Aviso"],
            MessageIcon.Info => StringLocalizer["Info"],
            _ => StringLocalizer["Message"],
        };

        return messageBox;
    }

    public JJMessageBox Create(string text, string title, MessageIcon icon, MessageSize size)
    {
        var messageBox = Create();
        messageBox.Title = title;
        messageBox.Text = text;
        messageBox.Icon = icon;
        messageBox.Size = size;

        return messageBox;
    }

    
}