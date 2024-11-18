

using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class SubmitAction : BasicAction
{
    [JsonPropertyName("formAction")]
    public string FormAction { get; set; }

    [JsonIgnore]
    public override bool IsCustomAction => true;
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}