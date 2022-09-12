using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace JJMasterData.Commons.Dao.Entity;

/// <summary>
/// Specific relationship information between tables
/// </summary>
/// <remarks>2017-03-22 JJTeam</remarks>
[Serializable]
[DataContract]
public class ElementRelation
{
    [DataMember(Name = "childElement")]
    public string ChildElement { get; set; }
        
    [DataMember(Name = "columns")]
    public List<ElementRelationColumn> Columns { get; set; }

    [DataMember(Name = "updateOnCascade")]
    public bool UpdateOnCascade { get; set; }

    [DataMember(Name = "deleteOnCascade")]
    public bool DeleteOnCascade { get; set; }

    [DataMember(Name = "viewType")]
    public RelationType ViewType { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }

    public ElementRelation()
    {
        Columns = new List<ElementRelationColumn>();
    }

    public ElementRelation(string childElement, params ElementRelationColumn[] columns)
    {
        Columns = new List<ElementRelationColumn>();
        ChildElement = childElement;
        ViewType = RelationType.List;
        if (columns != null)
            Columns.AddRange(columns);
    }

}