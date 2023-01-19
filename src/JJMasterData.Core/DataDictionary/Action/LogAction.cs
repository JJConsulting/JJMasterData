using System;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
public class LogAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ACTION_NAME = "log";

    public LogAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Log";
        Icon = IconType.Film;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 20;
        SetVisible(false);
    }
}