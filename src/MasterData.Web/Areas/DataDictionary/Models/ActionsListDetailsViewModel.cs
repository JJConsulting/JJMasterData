using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class ActionsListDetailsViewModel
{
    public required ActionSource Source { get; init; }
    public required List<BasicAction> Actions { get; init; }
    public string? FieldName { get; init; }
    public required string ElementName { get; set; }
}