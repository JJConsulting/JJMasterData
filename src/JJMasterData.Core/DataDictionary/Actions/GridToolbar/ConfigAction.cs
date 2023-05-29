using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

/// <summary>
/// Represents the settings menu of a data dictionary.
/// </summary>

public class ConfigAction : GridToolbarAction
{
    /// <summary>
    /// Default action name
    /// </summary>
    public const string ActionName = "config";
    public ConfigAction()
    {
        Name = ActionName;
        ToolTip = "Options";
        Icon = IconType.Cog;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 2;
    }
}