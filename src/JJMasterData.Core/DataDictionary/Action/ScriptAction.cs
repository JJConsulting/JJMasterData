using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class ScriptAction : BasicAction
{
    /// <summary>
    /// Ação JavaScript que será executada quando usuário clicar no controle
    /// </summary>
    [DataMember(Name = "onClientClick")]
    public string OnClientClick { get; set; }

    public ScriptAction()
    {
        IsCustomAction = true;
    }

}