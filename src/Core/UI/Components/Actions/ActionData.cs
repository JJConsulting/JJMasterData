#nullable enable

using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components;

internal record ActionData
{
    [JsonProperty("componentName")]
    public required string ComponentName { get; set; }
    
    [JsonProperty("actionMap")]
    public required string EncryptedActionMap { get; set; }
    
    [JsonProperty("modalTitle")]
    public string? ModalTitle { get; set; }
    
    [JsonProperty("gridViewRouteContext")]
    public string? EncryptedGridViewRouteContext { get; set; }
    
    [JsonProperty("confirmationMessage")]
    public string? ConfirmationMessage { get; set; }

    [JsonProperty("isSubmit")]
    public bool IsSubmit { get; set; }

    [JsonProperty("isModal")]
    public bool IsModal { get; set; }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        });
    }
}
