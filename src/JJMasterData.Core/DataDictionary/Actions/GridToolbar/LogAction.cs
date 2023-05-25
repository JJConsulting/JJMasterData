using JJMasterData.Core.DataDictionary.Actions.Abstractions;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class LogAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "log";
    public override bool IsUserCreated => false;
    public LogAction()
    {
        Name = ActionName;
        ToolTip = "Log";
        Icon = IconType.Film;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 20;
        SetVisible(false);
    }
}