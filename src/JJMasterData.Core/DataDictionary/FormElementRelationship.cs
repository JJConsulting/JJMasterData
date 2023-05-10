using System.Runtime.Serialization;
using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary;

public class FormElementRelationship
{
    public bool IsParent { get; private set; }

    public ElementRelationship ElementRelationship { get; set; }
    
    [DataMember(Name = "viewType")]
    public RelationshipType ViewType { get; set; }

    public FormElementPanel Panel { get; set; }

    internal FormElementRelationship(bool isParent = false)
    {
        IsParent = isParent;
        Panel = new FormElementPanel();
        Panel.VisibleExpression = "val:0";
    }

    public FormElementRelationship(ElementRelationship elementRelationship) : this()
    {
        ElementRelationship = elementRelationship;
    }

}