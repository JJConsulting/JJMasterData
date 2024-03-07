#nullable enable

using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Data.Entity.Models;
using Newtonsoft.Json;

namespace JJMasterData.Core.DataDictionary.Models;

public class FormElementRelationship
{
    [JsonProperty("id")]
    public int Id { get; set; }
    
    [JsonProperty("isParent")]
    public bool IsParent { get; set; }
    
    [JsonProperty("editModeOpenByDefault")]
    [Display(Name = "Edit Mode Open By Default")]
    public bool EditModeOpenByDefault { get; set; }

    [JsonProperty("elementRelationship")]
    public ElementRelationship? ElementRelationship { get; set; }
    
    [JsonProperty("viewType")]
    [Display(Name = "View Type")]
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
        return new FormElementRelationship
        {
            Id = Id,
            Panel = Panel.DeepCopy(),
            ElementRelationship = ElementRelationship?.DeepCopy(),
            ViewType = ViewType,
            IsParent = IsParent,
            EditModeOpenByDefault = EditModeOpenByDefault
        };
    }
}