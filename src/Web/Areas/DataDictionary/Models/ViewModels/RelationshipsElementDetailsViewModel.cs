using System.ComponentModel;
using JJMasterData.Commons.Data.Entity.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class RelationshipsElementDetailsViewModel : DataDictionaryViewModel
{
    public required int? Id { get; set; }
    public required ElementRelationship Relationship { get; set; }
    public required List<SelectListItem>? PrimaryKeysSelectList { get; set; }
    public required List<SelectListItem>? ForeignKeysSelectList { get; set; }
    public required List<SelectListItem>? ElementsSelectList { get; set; }
    
    [DisplayName("Primary Key Column")]
    public string? AddPrimaryKeyName { get; set; }
    [DisplayName("Foreign Key Column")]
    public string? AddForeignKeyName { get; set; }

    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    public RelationshipsElementDetailsViewModel()
    {
        
    }
    
    public RelationshipsElementDetailsViewModel(string elementName, string menuId) : base(elementName, menuId)
    {
    }
}