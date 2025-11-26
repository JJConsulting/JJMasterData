

using System.Text.Json.Serialization;
using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class InternalAction : BasicAction
{
    [JsonPropertyName("elementRedirect")]
    public FormActionRedirect ElementRedirect { get; set; }
    public InternalAction()
    {
        Icon = FontAwesomeIcon.ExternalLinkSquare;
        ElementRedirect = new FormActionRedirect();
    }

    [JsonIgnore]
    public override bool IsUserDefined => true;

    public override BasicAction DeepCopy()
    { 
        var newAction = (InternalAction)MemberwiseClone();
        newAction.ElementRedirect = ElementRedirect.DeepCopy();
        return newAction;
    }
}