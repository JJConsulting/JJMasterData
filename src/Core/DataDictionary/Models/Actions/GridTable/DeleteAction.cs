namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Represents the default delete action of a data dictionary
/// </summary>

public class DeleteAction : GridTableAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "delete";
    public DeleteAction()
    {
        Name = ActionName;
        Tooltip = "Delete";
        ConfirmationMessage = "Would you like to delete this record?";
        Icon = IconType.Trash;
        Order = 3;
    }
}