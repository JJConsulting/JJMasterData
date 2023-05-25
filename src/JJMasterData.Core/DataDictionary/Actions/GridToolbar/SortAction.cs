using System;
using System.Runtime.Serialization;
using JJMasterData.Core.Web;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
[DataContract]
public class SortAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "sort";
    public override bool IsUserCreated => false;
    public SortAction()
    {
        Name = ActionName;
        ToolTip = "Sort";
        Icon = IconType.SortAlphaAsc;
        ShowAsButton = true;
        CssClass = BootstrapHelper.PullRight;
        Order = 7;
        SetVisible(false);
    }
}