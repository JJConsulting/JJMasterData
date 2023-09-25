namespace JJMasterData.Core.DataDictionary.Models.Actions;

/// <summary>
/// Action to view a audit log entry from the toolbar.
/// </summary>
public class AuditLogFormToolbarAction : FormToolbarAction
{
    public const string ActionName = "audit-log-form-toolbar";
    
    public AuditLogFormToolbarAction()
    {
        Name = ActionName;
        Text = "View Audit Log";
        Icon = IconType.Film;
        ShowAsButton = true;
        VisibleExpression = "val:0";
        Order = 2;
        Location = FormToolbarActionLocation.Panel;
    }
}