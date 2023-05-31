namespace JJMasterData.Core.DataDictionary.Actions.GridTable;


public class EditAction : GridTableAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "edit";
    public EditAction()
    {
        Name = ActionName;
        ToolTip = "Edit";
        ConfirmationMessage = "";
        Icon = IconType.Pencil;
        Order = 2;
    }
}