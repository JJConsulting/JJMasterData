using System.ComponentModel.DataAnnotations;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class RelationshipsLayoutDetailsViewModel
{
    public required string ElementName { get; set; }
    public required int Id { get; set; }
    public required bool IsParent { get; set; }
    
    [Display(Name = "Edit Mode Open By Default")]
    public required bool EditModeOpenByDefault { get; set; }
    
    [Display(Name = "View Type")]
    public required RelationshipViewType ViewType { get; set; }
    
    public required FormElementPanel Panel { get; set; } = new();
}