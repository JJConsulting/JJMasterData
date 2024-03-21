using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class InternalAction : UserCreatedAction
{
    [JsonProperty("elementRedirect")]
    public FormActionRedirect ElementRedirect { get; set; }
    public InternalAction()
    {
        Icon = IconType.ExternalLinkSquare;
        ElementRedirect = new FormActionRedirect();
    }

    public override BasicAction DeepCopy()
    { 
        var newAction = (InternalAction)CopyAction();
        newAction.ElementRedirect = ElementRedirect.DeepCopy();
        return newAction;
    }
}