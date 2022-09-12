using System;
using System.Runtime.Serialization;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class FormActionRelationField
{
    [DataMember(Name = "internalField")]
    public string InternalField { get; set; }

    [DataMember(Name = "redirectField")]
    public string RedirectField { get; set; }
}