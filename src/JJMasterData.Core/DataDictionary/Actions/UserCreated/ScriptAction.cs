using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Actions.UserCreated;

public class ScriptAction : BasicAction
{
    /// <summary>
    /// Ação JavaScript que será executada quando usuário clicar no controle
    /// </summary>
    [JsonProperty("onClientClick")]
    public string OnClientClick { get; set; }
    public override bool IsUserCreated => true;
}