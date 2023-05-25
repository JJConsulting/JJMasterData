using System;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
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