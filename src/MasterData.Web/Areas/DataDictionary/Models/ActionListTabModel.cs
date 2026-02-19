using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public class ActionListTabModel
{
    public required string ElementName { get; set; }
    public required ActionSource Source { get; set; }
    public required List<BasicAction> Actions { get; set; }
    public required BasicAction SelectedAction { get; set; }
    public string SelectedActionKey => string.IsNullOrEmpty(SelectedFieldName) ? SelectedAction.Name : SelectedFieldName + "__" + SelectedAction.Name;

    public string? SelectedFieldName { get; set; }
}