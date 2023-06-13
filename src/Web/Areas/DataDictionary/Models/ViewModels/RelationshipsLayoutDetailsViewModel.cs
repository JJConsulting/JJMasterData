using JJMasterData.Commons.Data.Entity;
using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class RelationshipsLayoutDetailsViewModel : DataDictionaryViewModel
{
    public required int Id { get; set; }
    public required bool IsParent { get; set; }
    public required RelationshipViewType ViewType { get; set; }
    public required FormElementPanel?  Panel { get; set; }
    
    // ReSharper disable once UnusedMember.Global
    // Reason: Used for model binding.
    public RelationshipsLayoutDetailsViewModel()
    {
        
    }
    public RelationshipsLayoutDetailsViewModel(string dictionaryName, string menuId) : base(dictionaryName, menuId)
    {
    }
}