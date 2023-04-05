using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary;

[Serializable]
[DataContract]
public class FormActionRedirect
{
    [DataMember(Name = "elementNameRedirect")]
    public string ElementNameRedirect { get; set; }

    [DataMember(Name = "entityReferences")]
    public List<FormActionRelationField> RelationFields { get; set; }

    [DataMember(Name = "viewType")]
    public RelationshipType ViewType { get; set; }

    [DataMember(Name = "popupSize")]
    public PopupSize PopupSize { get; set; }


    public FormActionRedirect()
    {
        RelationFields = new List<FormActionRelationField>();
    }

        
}