namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class LogAction : GridToolbarAction
{
    public const string ActionName = "log";
    public LogAction()
    {
        Name = ActionName;
        Tooltip = "Audit Log";
        Icon = IconType.Film;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 20;
        SetVisible(false);
    }
}