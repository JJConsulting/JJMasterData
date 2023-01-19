using System;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Action;

/// <summary>
/// Represents the settings menu of a data dictionary.
/// </summary>
[Serializable]
public class ConfigAction : BasicAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ACTION_NAME = "config";

    public ConfigAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Options";
        Icon = IconType.Cog;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 2;
    }
}