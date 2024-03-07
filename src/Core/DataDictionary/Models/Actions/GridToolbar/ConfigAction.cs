﻿namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Represents the settings menu of a data dictionary.
/// </summary>

public class ConfigAction : GridToolbarAction
{
    public const string ActionName = "config";
    public ConfigAction()
    {
        Name = ActionName;
        Tooltip = "Options";
        Icon = IconType.Cog;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 2;
    }
    public override BasicAction DeepCopy() => CopyAction();
}