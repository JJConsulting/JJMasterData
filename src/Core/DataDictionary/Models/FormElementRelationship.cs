#nullable enable

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Data.Entity.Models;


namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementRelationship
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("isParent")]
    public bool IsParent { get; set; }
    
    [JsonPropertyName("editModeOpenByDefault")]
    [Display(Name = "Edit Mode Open By Default")]
    public bool EditModeOpenByDefault { get; set; }

    [JsonPropertyName("elementRelationship")]
    public ElementRelationship? ElementRelationship { get; set; }
    
    [JsonPropertyName("viewType")]
    [Display(Name = "View Type")]
    public RelationshipViewType ViewType { get; set; }

    [JsonPropertyName("panel")]
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
            Title = ElementRelationship?.ChildElement,
            Layout = PanelLayout.Collapse
        };
        if (!isParent)
            Panel.VisibleExpression = "exp:'{PageState}'<>'Insert'";
    }

    public FormElementRelationship(ElementRelationship elementRelationship) : this(false)
    {
        ElementRelationship = elementRelationship;
    }

    public FormElementRelationship DeepCopy()
    {
        var copy = (FormElementRelationship)MemberwiseClone();
        copy.Panel = Panel.DeepCopy();
        copy.ElementRelationship = ElementRelationship?.DeepCopy();
        return copy;
    }
}