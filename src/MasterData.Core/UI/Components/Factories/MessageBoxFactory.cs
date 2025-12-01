using System.Collections.Generic;
using System.Text;
using JJConsulting.Html.Bootstrap.Components;
using JJConsulting.Html.Bootstrap.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components.Factories;

public sealed class MessageBoxFactory(IStringLocalizer<MasterDataResources> stringLocalizer) 
{
    public JJMessageBox Create(string text, MessageIcon icon)
    {
        var messageBox = new JJMessageBox
        {
            Content = text,
            Icon = icon,
            Size = MessageSize.Default
        };
        messageBox.Title = messageBox.Icon switch
        {
            MessageIcon.Error => stringLocalizer["Error"],
            MessageIcon.Warning => stringLocalizer["Warning"],
            MessageIcon.Info => stringLocalizer["Info"],
            _ => stringLocalizer["Message"],
        };

        return messageBox;
    }

    public JJMessageBox Create(Dictionary<string, string> errors, MessageIcon icon)
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
}