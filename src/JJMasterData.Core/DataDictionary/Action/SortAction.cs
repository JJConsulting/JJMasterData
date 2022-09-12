using System;
using System.Runtime.Serialization;
using JJMasterData.Core.WebComponents;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class SortAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ACTION_NAME = "sort";

    public SortAction()
    {
        Name = ACTION_NAME;
        ToolTip = "Sort";
        Icon = IconType.SortAlphaAsc;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 7;
        SetVisible(false);
    }
}