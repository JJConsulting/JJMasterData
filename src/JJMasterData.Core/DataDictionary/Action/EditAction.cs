using System;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
public class EditAction : BasicAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ACTION_NAME = "edit";

    public EditAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Edit";
        ConfirmationMessage = "";
        Icon = IconType.Pencil;
        Order = 2;
    }
}