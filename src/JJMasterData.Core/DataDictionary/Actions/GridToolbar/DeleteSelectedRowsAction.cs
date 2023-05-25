using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class DeleteSelectedRowsAction : BasicAction
{
    /// <summary>
    /// Action name
    /// </summary>
    public const string ActionName = "deleteSelectedRows";
    public override bool IsUserCreated => false;
    public DeleteSelectedRowsAction()
    {
        Name = ActionName;
        Text = "Delete Selected";
        Icon = IconType.Trash;
        ShowAsButton = true;
        Order = 3;
        SetVisible(false);
    }
}