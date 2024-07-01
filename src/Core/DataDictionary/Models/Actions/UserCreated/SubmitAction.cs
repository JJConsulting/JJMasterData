using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;


public sealed class SubmitAction : UserCreatedAction
{
    [JsonProperty("formAction")]
    public string FormAction { get; set; }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}