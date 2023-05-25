using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.UserCreated;


public class SubmitAction : BasicAction
{
    [JsonProperty("formAction")]
    public string FormAction { get; set; }
    
    public override bool IsUserCreated => true;
}