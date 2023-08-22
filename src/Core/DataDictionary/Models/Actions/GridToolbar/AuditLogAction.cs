namespace JJMasterData.Core.DataDictionary.Actions.GridToolbar;

public class AuditLogAction : GridToolbarAction
{
    public const string ActionName = "log";
    public AuditLogAction()
    {
        Name = ActionName;
        ToolTip = "Audit Log";
        Icon = IconType.Film;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 20;
        SetVisible(false);
    }
}