using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class RefreshAction : GridToolbarAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "refresh";
    public RefreshAction()
    {
        Name = ActionName;
        ToolTip = "Refresh";
        Icon = IconType.Refresh;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 6;
    }
}