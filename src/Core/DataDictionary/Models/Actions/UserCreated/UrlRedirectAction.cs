using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class UrlRedirectAction : UserCreatedAction
{
    [JsonProperty("urlRedirect")]
    public string UrlRedirect { get; set; }

    
    [JsonProperty("urlAsPopUp")]
    public bool IsModal { get; set; }
    
    /// <summary>
    /// If the action is inside a modal, render as iFrame. If this is false, it will only add the resulting HTML inside the modal (recommended).
    /// </summary>
    [JsonProperty("isIframe")]
    public bool IsIframe { get; set; } = true;
    
    [JsonProperty("popupSize")]
    public ModalSize ModalSize { get; set; } = ModalSize.Default;

    [JsonProperty("titlePopUp")]
    public string ModalTitle { get; set; } = "Title";

    public UrlRedirectAction()
    {
        Icon = IconType.ExternalLink;
    }
}