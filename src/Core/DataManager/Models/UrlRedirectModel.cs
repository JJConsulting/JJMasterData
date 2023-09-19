#nullable enable

using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Models;

public record UrlRedirectModel
{
    [JsonProperty("urlAsPopUp")] public required bool UrlAsPopUp { get; init; }
    [JsonProperty("isIframe")] public required bool IsIframe { get; init; }
    [JsonProperty("popUpTitle")] public required string PopUpTitle { get; init; }
    [JsonProperty("urlRedirect")] public required string UrlRedirect { get; init; }
}