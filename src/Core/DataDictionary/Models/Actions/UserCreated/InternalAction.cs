using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;

public class InternalAction : UserCreatedAction
{
    [JsonProperty("elementRedirect")]
    public FormActionRedirect ElementRedirect { get; set; }
    public InternalAction()
    {
        Icon = IconType.ExternalLinkSquare;
        ElementRedirect = new FormActionRedirect();
    }

}