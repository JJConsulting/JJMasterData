using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions;

public class ScriptAction : UserCreatedAction
{
    /// <summary>
    /// JS script that will be executed when the user clicks on the control.
    /// </summary>
    [JsonProperty("onClientClick")]
    public string OnClientClick { get; set; }
    public override bool IsUserCreated => true;
}