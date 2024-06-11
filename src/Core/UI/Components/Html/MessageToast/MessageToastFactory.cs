using System;
using JJMasterData.Commons.Localization;
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
                messageToast.Icon.IconClass = IconType.TimesCircle.GetCssClass();
                messageToast.Title = stringLocalizer["Error"];
                break;
            case BootstrapColor.Warning:
                messageToast.Icon.IconClass = IconType.Warning.GetCssClass();
                messageToast.Title = stringLocalizer["Warning"];
                break;
            case BootstrapColor.Info:
                messageToast.Icon.IconClass = IconType.SolidCircleInfo.GetCssClass();
                messageToast.Title = stringLocalizer["Operation Performed"];
                break;
            default:
                messageToast.Icon.IconClass = IconType.Check.GetCssClass();
                messageToast.Title = stringLocalizer["Operation Performed"];
                break;
        }
       
        return messageToast;
    }
    
}