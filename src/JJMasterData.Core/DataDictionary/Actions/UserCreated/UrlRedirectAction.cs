using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.UserCreated;

public class UrlRedirectAction : UserCreatedAction
{
    [JsonProperty("urlRedirect")]
    public string UrlRedirect { get; set; }

    [JsonProperty("urlAsPopUp")]
    public bool UrlAsPopUp { get; set; }

    [JsonProperty("titlePopUp")]
    public string TitlePopUp { get; set; }

    public UrlRedirectAction()
    {
        UrlAsPopUp = false;
        TitlePopUp = "Title";
        Icon = IconType.ExternalLink;
    }
}