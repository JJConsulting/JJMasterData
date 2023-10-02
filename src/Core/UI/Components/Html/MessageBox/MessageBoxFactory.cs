using System.Collections.Generic;
using System.Text;
using JJMasterData.Commons.Localization;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class MessageBoxFactory : IComponentFactory<JJMessageBox>
{
    public IStringLocalizer<JJMasterDataResources> StringLocalizer { get; }

    public MessageBoxFactory(IStringLocalizer<JJMasterDataResources> stringLocalizer)
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
        messageBox.Content = text;
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

    public JJMessageBox Create(IDictionary<string, string> errors, MessageIcon icon)
    {
        var errorMessage = new StringBuilder();
        foreach (var err in errors.Values)
        {
            errorMessage.Append("- ");
            errorMessage.Append(err);
            errorMessage.AppendLine("<br>");
        }

        return Create(errorMessage.ToString(), icon);
    }

    public JJMessageBox Create(string title, string text, MessageIcon icon, MessageSize size)
    {
        var messageBox = Create();
        messageBox.Title = title;
        messageBox.Content = text;
        messageBox.Icon = icon;
        messageBox.Size = size;

        return messageBox;
    }

    
}