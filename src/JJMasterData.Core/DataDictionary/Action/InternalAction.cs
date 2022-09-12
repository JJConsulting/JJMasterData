using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class InternalAction : BasicAction
{
    [DataMember(Name = "elementRedirect")]
    public FormActionRedirect ElementRedirect { get; set; }

    public InternalAction()
    {
        Icon = IconType.ExternalLinkSquare;
        ElementRedirect = new FormActionRedirect();
        IsCustomAction = true;
    }

}