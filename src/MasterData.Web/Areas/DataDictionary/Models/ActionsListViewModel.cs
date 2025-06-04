using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class ActionsListViewModel
{
    public required string ElementName { get; init; }
    public required List<BasicAction> GridTableActions { get; init; }
    public required List<BasicAction> GridToolbarActions { get;  init; }
    public required List<BasicAction> FormToolbarActions { get;  init; }
}