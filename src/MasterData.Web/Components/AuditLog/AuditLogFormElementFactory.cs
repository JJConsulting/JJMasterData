using JJConsulting.FontAwesome;
using JJMasterData.Commons.Data.Entity.Models;
using JJMasterData.Core.DataDictionary.Models;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.DataManager.Models;
using JJMasterData.Core.DataManager.Services;
using JJMasterData.Web.Utils;

namespace JJMasterData.Web.Components;

public sealed class AuditLogFormElementFactory(AuditLogService auditLogService)
{
    internal static string GetUpdateColor() => BootstrapHelper.Version == 3 ? "#ffbf00" : "var(--bs-warning)";
    internal static string GetInsertColor() => BootstrapHelper.Version == 3 ? "#387c44" : "var(--bs-success)";
    internal static string GetDeleteColor() => BootstrapHelper.Version == 3 ? "#b20000" : "var(--bs-danger)";

    public FormElement Create(FormElement formElement)
    {
        var auditLogFormElement = new FormElement(auditLogService.GetElement());
        auditLogFormElement.Fields[AuditLogService.DicId].VisibleExpression = "val:0";
        auditLogFormElement.Fields[AuditLogService.DicName].VisibleExpression = "val:0";
        auditLogFormElement.Fields[AuditLogService.DicBrowser].VisibleExpression = "val:0";
        auditLogFormElement.Fields[AuditLogService.DicJson].VisibleExpression = "val:0";
        auditLogFormElement.Fields[AuditLogService.DicModified].Component = FormComponent.DateTime;
        auditLogFormElement.Options.GridTableActions.Clear();
        auditLogFormElement.Options.GridToolbarActions.InsertAction.SetVisible(false);

        var origin = auditLogFormElement.Fields[AuditLogService.DicOrigin];
        origin.Component = FormComponent.ComboBox;
        origin.DataItem = new FormElementDataItem { GridBehavior = DataItemGridBehavior.Icon, Items = [] };
        foreach (int value in Enum.GetValues<DataContextSource>())
            origin.DataItem.Items.Add(new DataItemValue(value.ToString(), Enum.GetName(typeof(DataContextSource), value)!));

        var action = auditLogFormElement.Fields[AuditLogService.DicAction];
        action.Component = FormComponent.ComboBox;
        action.DataItem = new FormElementDataItem
        {
            GridBehavior = DataItemGridBehavior.Icon,
            Items = [],
            ShowIcon = true
        };
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Insert).ToString(), "Added",
            FontAwesomeIcon.Plus, GetInsertColor()));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Update).ToString(), "Edited",
            FontAwesomeIcon.Pencil, GetUpdateColor()));
        action.DataItem.Items.Add(new DataItemValue(((int)CommandOperation.Delete).ToString(), "Deleted",
            FontAwesomeIcon.Trash, GetDeleteColor()));

        var viewLog = new ScriptAction
        {
            Name = "btnViewLog",
            Icon = FontAwesomeIcon.Eye,
            Tooltip = "View",
            OnClientClick = $"AuditLogViewHelper.viewAuditLog('{formElement.Name}','{{{AuditLogService.DicId}}}');"
        };
        auditLogFormElement.Options.GridTableActions.Add(viewLog);
        return auditLogFormElement;
    }
}
