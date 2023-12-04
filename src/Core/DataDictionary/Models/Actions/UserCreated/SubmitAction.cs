using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public class SubmitAction : UserCreatedAction
{
    [JsonProperty("formAction")]
    public string FormAction { get; set; }
    
}