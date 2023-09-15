#nullable enable

using Newtonsoft.Json;

namespace JJMasterData.Core.UI.Components.Actions;

internal record ActionData
{
    [JsonProperty("componentName")]
    public required string ComponentName { get; set; }
    
    [JsonProperty("actionMap")]
    public required string EncryptedActionMap { get; set; }
    
    [JsonProperty("modalTitle")]
    public string? ModalTitle { get; set; }
    
    [JsonProperty("modalRouteContext")]
    public string? EncryptedModalRouteContext { get; set; }
    
    [JsonProperty("formViewRouteContext")]
    public string? EncryptedFormViewRouteContext { get; set; }
    
    [JsonProperty("confirmationMessage")]
    public string? ConfirmationMessage { get; set; }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this, new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore
        });
    }
}
