using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class UrlRedirectAction : BasicAction
{
    [JsonPropertyName("urlRedirect")]
    [Display(Name = "Url")]
    public string UrlRedirect { get; set; }
    
    [JsonPropertyName("urlAsPopUp")]
    [Display(Name = "Is Modal?")]
    public bool IsModal { get; set; }
    
    /// <summary>
    /// If the action is inside a modal, render as iFrame. If this is false, it will only add the resulting HTML inside the modal (recommended).
    /// </summary>
    [JsonPropertyName("isIframe")]
    [Display(Name = "Is Iframe?")]
    public bool IsIframe { get; set; } = true;
    
    [JsonPropertyName("popupSize")]
    public ModalSize ModalSize { get; set; } = ModalSize.Default;

    [JsonPropertyName("ModalTitle")]
    [Display(Name = "Modal Title")]
    public string ModalTitle { get; set; } = "Title";

    [JsonPropertyName("encryptParameters")]
    [Display(Name="Encrypt Parameters")]
    public bool EncryptParameters { get; set; }
    
    [JsonPropertyName("openInNewTab")]
    [Display(Name="Open in New Tab")]
    public bool OpenInNewTab { get; set; }
    
    public UrlRedirectAction()
    {
        Icon = IconType.ExternalLink;
    }
    [JsonIgnore]
    public override bool IsCustomAction => true;
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}