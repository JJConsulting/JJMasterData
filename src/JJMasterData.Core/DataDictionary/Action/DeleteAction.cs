using System;

namespace JJMasterData.Core.DataDictionary.Action;

/// <summary>
/// Represents the default delete action of a data dictionary
/// </summary>
[Serializable]
public class DeleteAction : BasicAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ACTION_NAME = "delete";

    public DeleteAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Delete";
        ConfirmationMessage = "Would you like to delete this record?";
        Icon = IconType.Trash;
        Order = 3;
    }
}