using JJConsulting.FontAwesome;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class AuditLogGridToolbarAction : GridToolbarAction
{
    public const string ActionName = "audit-log-grid-toolbar";
    public AuditLogGridToolbarAction()
    {
        Name = ActionName;
        Tooltip = "Audit Log";
        Icon = FontAwesomeIcon.Film;
        ShowAsButton = true;
        CssClass = "float-end";
        Order = 20;
        SetVisible(false);
    }
    public override BasicAction DeepCopy() => (BasicAction)MemberwiseClone();
}