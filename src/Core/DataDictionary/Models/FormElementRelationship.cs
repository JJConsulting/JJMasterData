#nullable enable
using JJMasterData.Commons.Data.Entity;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary;

public class FormElementRelationship
{
    [JsonProperty("id")]
    public int Id { get; set; }
    
    [JsonProperty("isParent")]
    public bool IsParent { get; set; }

    [JsonProperty("elementRelationship")]
    public ElementRelationship? ElementRelationship { get; set; }
    
    [JsonProperty("viewType")]
    public RelationshipViewType ViewType { get; set; }

    [JsonProperty("panel")]
    public FormElementPanel Panel { get; set; } = null!;


    [JsonConstructor]
    private FormElementRelationship()
    {
        
    }
    
    public FormElementRelationship(bool isParent = false)
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