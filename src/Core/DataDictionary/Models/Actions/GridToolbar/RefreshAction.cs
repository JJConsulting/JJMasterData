namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class RefreshAction : GridToolbarAction
{
    public const string ActionName = "refresh";
    public RefreshAction()
    {
        Name = ActionName;
        ToolTip = "Refresh";
        Icon = IconType.Refresh;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 6;
    }
}