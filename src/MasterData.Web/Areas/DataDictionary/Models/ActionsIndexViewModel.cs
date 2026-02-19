using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public sealed class ActionsIndexViewModel
{
    public required string ElementName { get; init; }
    public required List<BasicAction> GridTableActions { get; init; }
    public required List<BasicAction> GridToolbarActions { get;  init; }
    public required List<BasicAction> FormToolbarActions { get;  init; }
    public required List<FieldActionItem> FieldActions { get; init; }
}
public sealed class FieldActionItem
{
    public required string FieldName { get; init; }
    public required BasicAction Action { get; init; }
}
