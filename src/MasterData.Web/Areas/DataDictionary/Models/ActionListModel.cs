using JJMasterData.Core.DataDictionary.Models.Actions;

namespace JJMasterData.Web.Areas.DataDictionary.Models;

public class ActionListModel
{
    public required string ElementName { get; set; }
    public required ActionSource Source { get; set; }
    public required List<BasicAction> Actions { get; set; }
    public required BasicAction? SelectedAction { get; set; }
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

    public string? SelectedFieldName { get; set; }
}