using System;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
public class ViewAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ACTION_NAME = "view";

    public ViewAction()
    {
        Name = ACTION_NAME;
        ToolTip = "View";
        ConfirmationMessage = "";
        IsDefaultOption = true;
        Icon = IconType.Eye;
        Order = 1;
    }
}