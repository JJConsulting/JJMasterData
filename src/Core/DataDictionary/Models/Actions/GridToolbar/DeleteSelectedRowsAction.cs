namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class DeleteSelectedRowsAction : GridToolbarAction
{
    /// <summary>
    /// Action name
    /// </summary>
    public const string ActionName = "deleteSelectedRows";

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