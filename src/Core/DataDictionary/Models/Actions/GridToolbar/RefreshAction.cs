namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class RefreshAction : GridToolbarAction
{
    public const string ActionName = "refresh";
    public RefreshAction()
    {
        Name = ActionName;
        Tooltip = "Refresh";
        Icon = IconType.Refresh;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 6;
    }
    public override BasicAction DeepCopy() => CopyAction();
}