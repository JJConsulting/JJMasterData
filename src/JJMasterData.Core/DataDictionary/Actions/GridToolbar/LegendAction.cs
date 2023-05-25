using System;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
public class LegendAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "legend";
    public override bool IsUserCreated => false;
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