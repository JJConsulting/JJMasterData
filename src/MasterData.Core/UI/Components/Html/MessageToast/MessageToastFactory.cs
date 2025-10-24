using System;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.DataDictionary.Models;
using Microsoft.Extensions.Localization;

namespace JJMasterData.Core.UI.Components;

public class MessageToastFactory(IStringLocalizer<MasterDataResources> stringLocalizer) : IComponentFactory<JJMessageToast>
{
    public JJMessageToast Create()
    {
        return new JJMessageToast();
    }
    
    public JJMessageToast Create(string message, BootstrapColor color = BootstrapColor.Default)
    {
        var messageToast = Create();
        messageToast.Message = message;
        messageToast.TitleMuted = DateTime.Now.ToShortTimeString();
        messageToast.TitleColor = color;
        messageToast.Icon = new JJIcon();
        
        switch (color)
        {
            case BootstrapColor.Danger:
                messageToast.Icon.IconClass = "fa fa-times-circle";
                messageToast.Title = stringLocalizer["Error"];
                break;
            case BootstrapColor.Warning:
                messageToast.Icon.IconClass = "fa fa-warning";
                messageToast.Title = stringLocalizer["Warning"];
                break;
            case BootstrapColor.Info:
                messageToast.Icon.IconClass = "fa-solid fa-circle-info";
                messageToast.Title = stringLocalizer["Operation Performed"];
                break;
            default:
                messageToast.Icon.IconClass = "fa fa-check";
                messageToast.Title = stringLocalizer["Operation Performed"];
                break;
        }
       
        return messageToast;
    }
    
}