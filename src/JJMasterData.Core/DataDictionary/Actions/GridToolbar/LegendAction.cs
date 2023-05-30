using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class LegendAction : GridToolbarAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "legend";
    public LegendAction()
    {
        Name = ActionName;
        ToolTip = "Information";
        Icon = IconType.Info;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 7;
        SetVisible(false);
    }
}