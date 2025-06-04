

using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class InternalAction : BasicAction
{
    [JsonPropertyName("elementRedirect")]
    public FormActionRedirect ElementRedirect { get; set; }
    public InternalAction()
    {
        Icon = IconType.ExternalLinkSquare;
        ElementRedirect = new FormActionRedirect();
    }

    [JsonIgnore]
    public override bool IsCustomAction => true;

    public override BasicAction DeepCopy()
    { 
        var newAction = (InternalAction)MemberwiseClone();
        newAction.ElementRedirect = ElementRedirect.DeepCopy();
        return newAction;
    }
}