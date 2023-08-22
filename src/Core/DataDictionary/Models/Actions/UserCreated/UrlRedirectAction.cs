using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.UserCreated;

public class UrlRedirectAction : UserCreatedAction
{
    [JsonProperty("urlRedirect")]
    public string UrlRedirect { get; set; }

    [JsonProperty("urlAsPopUp")]
    public bool UrlAsPopUp { get; set; }

    [JsonProperty("titlePopUp")]
    public string PopUpTitle { get; set; }

    [JsonProperty("popupSize")]
    public PopupSize PopupSize { get; set; }
    
    public UrlRedirectAction()
    {
        UrlAsPopUp = false;
        PopUpTitle = "Title";
        PopupSize = PopupSize.Default;
        Icon = IconType.ExternalLink;
    }
}