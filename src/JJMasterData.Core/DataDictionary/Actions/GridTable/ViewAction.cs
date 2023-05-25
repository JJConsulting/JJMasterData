using JJMasterData.Core.DataDictionary.Actions.Abstractions;

namespace JJMasterData.Core.DataDictionary.Actions.GridTable;


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