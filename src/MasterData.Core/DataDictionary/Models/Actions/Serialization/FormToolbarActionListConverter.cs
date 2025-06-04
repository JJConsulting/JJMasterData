using System.Text.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

internal sealed class FormToolbarActionListConverter : ActionListConverterBase<FormToolbarActionList>
{
    protected override FormToolbarActionList ReadActionsFromLegacyFormat(JsonElement rootElement)
    {
        var formToolbarActionList = new FormToolbarActionList();
        foreach (var actionElement in rootElement.EnumerateArray())
        {
            var type = actionElement.GetProperty("$type").GetString();
            switch (type)
            {
                case "JJMasterData.Core.DataDictionary.Models.Actions.SaveAction, JJMasterData.Core":
                    formToolbarActionList.SaveAction = actionElement.Deserialize<SaveAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.BackAction, JJMasterData.Core":
                    formToolbarActionList.BackAction = actionElement.Deserialize<BackAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.CancelAction, JJMasterData.Core":
                    formToolbarActionList.CancelAction = actionElement.Deserialize<CancelAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.FormEditAction, JJMasterData.Core":
                    formToolbarActionList.FormEditAction = actionElement.Deserialize<FormEditAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.AuditLogFormToolbarAction, JJMasterData.Core":
                    formToolbarActionList.AuditLogFormToolbarAction = actionElement.Deserialize<AuditLogFormToolbarAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.PluginAction, JJMasterData.Core":
                    formToolbarActionList.Set(actionElement.Deserialize<PluginAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.HtmlTemplateAction, JJMasterData.Core":
                    formToolbarActionList.Set(actionElement.Deserialize<HtmlTemplateAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.InternalAction, JJMasterData.Core":
                    formToolbarActionList.Set(actionElement.Deserialize<InternalAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ScriptAction, JJMasterData.Core":
                    formToolbarActionList.Set(actionElement.Deserialize<ScriptAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SqlCommandAction, JJMasterData.Core":
                    formToolbarActionList.Set(actionElement.Deserialize<SqlCommandAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.UrlRedirectAction, JJMasterData.Core":
                    formToolbarActionList.Set(actionElement.Deserialize<UrlRedirectAction>());
                    break;
            }
        }

        return formToolbarActionList;
    }

    protected override FormToolbarActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options)
    {
        var formToolbarActionList = new FormToolbarActionList
        {
            SaveAction = rootElement.GetProperty("saveAction").Deserialize<SaveAction>(options),
            BackAction = rootElement.GetProperty("backAction").Deserialize<BackAction>(options),
            CancelAction = rootElement.GetProperty("cancelAction").Deserialize<CancelAction>(options),
            FormEditAction =
                rootElement.GetProperty("formEditAction").Deserialize<FormEditAction>(options),
            AuditLogFormToolbarAction =
                rootElement.GetProperty("auditLogFormToolbarAction").Deserialize<AuditLogFormToolbarAction>(options)
        };
        return formToolbarActionList;
    }

    protected override void WriteActions(Utf8JsonWriter writer, FormToolbarActionList actionListToWrite,
        JsonSerializerOptions options)
    {
        WriteProperty(writer, "saveAction", actionListToWrite.SaveAction, options);
        WriteProperty(writer, "backAction", actionListToWrite.BackAction, options);
        WriteProperty(writer, "cancelAction", actionListToWrite.CancelAction, options);
        WriteProperty(writer, "formEditAction", actionListToWrite.FormEditAction, options);
        WriteProperty(writer, "auditLogFormToolbarAction", actionListToWrite.AuditLogFormToolbarAction, options);
    }
}