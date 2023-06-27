namespace JJMasterData.Core.DataDictionary.Actions.GridTable;


public class ViewAction : GridTableAction
{
    /// <summary>
    /// Nome padrão da ação
    /// </summary>
    public const string ActionName = "view";
    public bool ShowAsPopup { get; set; }
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