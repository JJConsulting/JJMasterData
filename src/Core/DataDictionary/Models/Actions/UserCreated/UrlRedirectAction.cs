using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class UrlRedirectAction : UserCreatedAction
{
    [JsonProperty("urlRedirect")]
    [Display(Name = "Url")]
    public string UrlRedirect { get; set; }
    
    [JsonProperty("urlAsPopUp")]
    [Display(Name = "Is Modal?")]
    public bool IsModal { get; set; }
    
    /// <summary>
    /// If the action is inside a modal, render as iFrame. If this is false, it will only add the resulting HTML inside the modal (recommended).
    /// </summary>
    [JsonProperty("isIframe")]
    [Display(Name = "Is Iframe?")]
    public bool IsIframe { get; set; } = true;
    
    [JsonProperty("popupSize")]
    public ModalSize ModalSize { get; set; } = ModalSize.Default;

    [JsonProperty("ModalTitle")]
    [Display(Name = "Modal Title")]
    public string ModalTitle { get; set; } = "Title";

    [JsonProperty("encryptParameters")]
    [Display(Name="Encrypt Parameters")]
    public bool EncryptParameters { get; set; }
    
    public UrlRedirectAction()
    {
        Icon = IconType.ExternalLink;
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}