using JJMasterData.Commons.Data.Entity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class RelationshipsDetailsViewModel : DataDictionaryViewModel
{
    public required int? SelectedIndex { get; init; }
    public required ElementRelationship Relationship { get; init; }
    public required List<SelectListItem> PrimaryKeysSelectList { get; init; }
    public required List<SelectListItem> ForeignKeysSelectList { get; init; }
    public required List<SelectListItem> ElementsSelectList { get; init; }
    public RelationshipsDetailsViewModel(string dictionaryName, string menuId) : base(dictionaryName, menuId)
    {
    }
}