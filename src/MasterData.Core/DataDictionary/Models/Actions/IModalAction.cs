using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public interface IModalAction
{
    [JsonPropertyName("showAsModal")]
    public bool ShowAsModal { get; set; }    
    [JsonPropertyName("modalTitle")]
    public string ModalTitle { get; set; }    
}