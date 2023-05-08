using JJMasterData.Commons.Data.Entity;

namespace JJMasterData.Core.DataDictionary;

public class FormElementRelationship
{
    public bool IsOwner { get; private set; }

    public ElementRelationship ElementRelation { get; set; }

    public FormElementPanel Panel { get; set; }

    internal FormElementRelationship(bool isOwner = false)
    {
        IsOwner = isOwner;
        Panel = new FormElementPanel();
    }

    public FormElementRelationship(ElementRelationship elementRelation) : this()
    {
        ElementRelation = elementRelation;
    }

}