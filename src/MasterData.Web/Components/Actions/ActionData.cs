using System.Text.Json.Serialization;

namespace JJMasterData.Web.Components;

internal class ActionData
{
    [JsonPropertyName("componentName")]
    public required string ComponentName { get; set; }
    
    [JsonPropertyName("actionMap")]
    public required string EncryptedActionMap { get; set; }
    
    [JsonPropertyName("modalTitle")]
    public string? ModalTitle { get; set; }
    
    [JsonPropertyName("gridViewRouteContext")]
    public string? EncryptedGridViewRouteContext { get; set; }
    
    [JsonPropertyName("confirmationMessage")]
    public string? ConfirmationMessage { get; set; }

    [JsonPropertyName("isSubmit")]
    public bool IsSubmit { get; set; }

    [JsonPropertyName("isModal")]
    public bool IsModal { get; set; }
}
