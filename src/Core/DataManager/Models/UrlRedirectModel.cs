using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Models;

public record UrlRedirectModel(
    [property: JsonProperty("urlAsPopUp")] bool UrlAsPopUp,
    [property: JsonProperty("popUpTitle")] string PopUpTitle, [property: JsonProperty("urlRedirect")]
    string UrlRedirect);