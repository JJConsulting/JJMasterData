using System;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
public class RefreshAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ACTION_NAME = "refresh";

    public RefreshAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Refresh";
        Icon = IconType.Refresh;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 6;
    }
}