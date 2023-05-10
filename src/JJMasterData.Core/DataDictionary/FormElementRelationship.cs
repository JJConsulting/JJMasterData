using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary;

public class FormElementRelationship
{
    [DataMember(Name = "isParent")]
    public bool IsParent { get; set; }

    [DataMember(Name = "elementRelationship")]
    public ElementRelationship ElementRelationship { get; set; }
    
    [DataMember(Name = "viewType")]
    public RelationshipViewType ViewType { get; set; }

    [DataMember(Name = "panel")]
    public FormElementPanel Panel { get; set; }

    internal FormElementRelationship(bool isParent = false)
    {
        IsParent = isParent;
        Panel = new FormElementPanel
        {
            VisibleExpression = "val:1",
        };
    }

    public FormElementRelationship(ElementRelationship elementRelationship) : this()
    {
        ElementRelationship = elementRelationship;
    }

}