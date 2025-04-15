using JJMasterData.Core.DataDictionary.Models;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class RelationshipsListViewModel
{
    public required string ElementName { get; set; }
    public required FormElementRelationshipList Relationships { get; init; }
}