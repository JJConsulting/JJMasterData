using System;

namespace JJMasterData.Core.DataDictionary.Action;

[Serializable]
public class ViewAction : BasicAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "view";
    public override bool IsUserCreated => false;
    public ViewAction()
    {
        Name = ActionName;
        ToolTip = "View";
        ConfirmationMessage = "";
        IsDefaultOption = true;
        Icon = IconType.Eye;
        Order = 1;
    }


}