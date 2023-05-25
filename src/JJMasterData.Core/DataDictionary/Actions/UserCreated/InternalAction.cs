using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.UserCreated;


public class InternalAction : BasicAction
{
    [JsonProperty("elementRedirect")]
    public FormActionRedirect ElementRedirect { get; set; }
    public override bool IsUserCreated => true;
    public InternalAction()
    {
        Icon = IconType.ExternalLinkSquare;
        ElementRedirect = new FormActionRedirect();
    }

}