using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Data.Entity;

/// <summary>
/// Specific relationship information between tables
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class ElementRelationship
{
    [DataMember(Name = "childElement")]
    public string ChildElement { get; set; }
        
    [DataMember(Name = "columns")]
    public List<ElementRelationshipColumn> Columns { get; set; }

    [DataMember(Name = "updateOnCascade")]
    public bool UpdateOnCascade { get; set; }

    [DataMember(Name = "deleteOnCascade")]
    public bool DeleteOnCascade { get; set; }

    [DataMember(Name = "viewType")]
    public RelationshipType ViewType { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }

    public ElementRelationship()
    {
        Columns = new List<ElementRelationshipColumn>();
    }

    public ElementRelationship(string childElement, params ElementRelationshipColumn[] columns)
    {
        Columns = new List<ElementRelationshipColumn>();
        ChildElement = childElement;
        ViewType = RelationshipType.List;
        if (columns != null)
            Columns.AddRange(columns);
    }

}