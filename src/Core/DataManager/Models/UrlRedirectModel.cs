#nullable enable

using JJMasterData.Core.DataDictionary.Models;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataManager.Models;

public record UrlRedirectModel
{
    [JsonProperty("urlAsModal")] 
    public required bool UrlAsModal { get; init; }
    
    [JsonProperty("isIframe")]
    public required bool IsIframe { get; init; }
    
    [JsonProperty("modalTitle")] 
    public required string ModalTitle { get; init; }
    
    [JsonProperty("urlRedirect")] 
    public required string UrlRedirect { get; init; }
    
    [JsonProperty("modalSize")] 
    public required ModalSize ModalSize { get; set; }
}