using JJMasterData.Core.DataDictionary;

namespace JJMasterData.Web.Areas.DataDictionary.Models.ViewModels;

public class RelationshipsListViewModel : DataDictionaryViewModel
{
    public required FormElementRelationshipList Relationships { get; init; }

    public RelationshipsListViewModel(string elementName, string menuId) : base(elementName, menuId)
    {
    }
}