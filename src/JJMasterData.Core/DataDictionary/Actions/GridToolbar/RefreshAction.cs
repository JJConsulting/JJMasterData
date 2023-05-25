using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class RefreshAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "refresh";
    public override bool IsUserCreated => false;
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