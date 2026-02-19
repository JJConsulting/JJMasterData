using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public class ActionListModel
{
    public required string ElementName { get; init; }
    public required ActionSource Source { get; init; }
    public required List<BasicAction> Actions { get; init; }
    public required BasicAction? SelectedAction { get; init; }
    public string? SelectedFieldName { get; init; }
    public string? SelectedActionKey
    {
        get
        {
            if (SelectedAction is null)
                return null;

            if (string.IsNullOrEmpty(SelectedFieldName))
                return SelectedAction?.Name;
            
            return SelectedFieldName + "__" + SelectedAction?.Name;
        }
    }
}