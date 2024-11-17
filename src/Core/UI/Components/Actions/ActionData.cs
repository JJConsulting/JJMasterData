#nullable enable


using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace JJMasterData.Core.UI.Components;

internal record ActionData
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

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}
