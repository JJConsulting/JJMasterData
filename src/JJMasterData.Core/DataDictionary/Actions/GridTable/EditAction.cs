using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Core.DataDictionary.Actions.GridTable;


public class EditAction : BasicAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "edit";
    public override bool IsUserCreated => false;
    public EditAction()
    {
        Name = ActionName;
        ToolTip = "Edit";
        ConfirmationMessage = "";
        Icon = IconType.Pencil;
        Order = 2;
    }
}