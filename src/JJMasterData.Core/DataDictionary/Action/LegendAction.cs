using System;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
public class LegendAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ACTION_NAME = "legend";

    public LegendAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Information";
        Icon = IconType.Info;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 7;
        SetVisible(false);
    }
}