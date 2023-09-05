#nullable enable

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JJMasterData.Core.UI.Components.Actions;

public record ActionData
{
    [JsonProperty("componentName")]
    public required string ComponentName { get; set; }
    
    [JsonProperty("actionMap")]
    public required string EncryptedActionMap { get; set; }
    
    [JsonProperty("modalTitle")]
    public string? ModalTitle { get; set; }
    
    [JsonProperty("modalRouteContext")]
    public string? EncryptedModalRouteContext { get; set; }
    
    [JsonProperty("gridRouteContext")]
    public string? EncryptedGridRouteContext { get; set; }
    
    [JsonProperty("confirmationMessage")]
    public string? ConfirmationMessage { get; set; }
}
