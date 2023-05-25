using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.UserCreated;


public class SubmitAction : UserCreatedAction
{
    [JsonProperty("formAction")]
    public string FormAction { get; set; }
    
}