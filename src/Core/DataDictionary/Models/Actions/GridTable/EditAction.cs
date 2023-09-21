namespace JJMasterData.Core.DataDictionary.Actions.GridTable;


public class EditAction : GridTableAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "edit";
    public bool ShowAsModal { get; set; }
    public EditAction()
    {
        Name = ActionName;
        Tooltip = "Edit";
        ConfirmationMessage = "";
        Icon = IconType.Pencil;
        Order = 2;
    }


}