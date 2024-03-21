namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class AuditLogGridToolbarAction : GridToolbarAction
{
    public const string ActionName = "audit-log-grid-toolbar";
    public AuditLogGridToolbarAction()
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