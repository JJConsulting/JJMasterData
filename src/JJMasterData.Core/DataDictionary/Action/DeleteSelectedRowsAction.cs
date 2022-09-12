using System;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
public class DeleteSelectedRowsAction : BasicAction
{
    /// <summary>
    /// Action name
    /// </summary>
    public const string ACTION_NAME = "deleteSelectedRows";

    public DeleteSelectedRowsAction()
    {
        Name = ACTION_NAME;
        Text = "Delete Selected";
        Icon = IconType.Trash;
        ShowAsButton = true;
        Order = 3;
        SetVisible(false);
    }
}