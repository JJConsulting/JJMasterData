using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class UrlRedirectAction : BasicAction
{
    [DataMember(Name = "urlRedirect")]
    public string UrlRedirect { get; set; }

    [DataMember(Name = "urlAsPopUp")]
    public bool UrlAsPopUp { get; set; }

    [DataMember(Name = "titlePopUp")]
    public string TitlePopUp { get; set; }

    [DataMember(Name = "popupSize")] 
    public PopupSize PopupSize { get; set; } = PopupSize.Full;

    public UrlRedirectAction()
    {
        UrlAsPopUp = false;
        TitlePopUp = "Title";
        Icon = IconType.ExternalLink;
        IsCustomAction = true;
    }
}