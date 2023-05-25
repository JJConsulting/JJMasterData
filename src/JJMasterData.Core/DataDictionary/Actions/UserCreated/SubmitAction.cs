using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class SubmitAction : BasicAction
{
    [DataMember(Name = "formAction")]
    public string FormAction { get; set; }
    
    public override bool IsUserCreated => true;
}