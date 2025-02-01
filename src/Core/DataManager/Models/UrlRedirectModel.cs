#nullable enable

using System.Text.Json.Serialization;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Core.DataManager.Models;

public class UrlRedirectModel
{
    [JsonPropertyName("urlAsModal")] 
    public required bool UrlAsModal { get; init; }
    
    [JsonPropertyName("isIframe")]
    public required bool IsIframe { get; init; }
    
    [JsonPropertyName("modalTitle")] 
    public required string ModalTitle { get; init; }
    
    [JsonPropertyName("urlRedirect")] 
    public required string UrlRedirect { get; init; }
    
    [JsonPropertyName("modalSize")] 
    public required ModalSize ModalSize { get; set; }
    
    [JsonPropertyName("OpenInNewTab")] 
    public bool OpenInNewTab { get; set; }
}