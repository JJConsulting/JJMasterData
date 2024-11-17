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
                    formToolbarActionList.SaveAction =
                        JsonSerializer.Deserialize<SaveAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.BackAction, JJMasterData.Core":
                    formToolbarActionList.BackAction =
                        JsonSerializer.Deserialize<BackAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.CancelAction, JJMasterData.Core":
                    formToolbarActionList.CancelAction =
                        JsonSerializer.Deserialize<CancelAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.FormEditAction, JJMasterData.Core":
                    formToolbarActionList.FormEditAction =
                        JsonSerializer.Deserialize<FormEditAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.AuditLogFormToolbarAction, JJMasterData.Core":
                    formToolbarActionList.AuditLogFormToolbarAction =
                        JsonSerializer.Deserialize<AuditLogFormToolbarAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.HtmlTemplateAction, JJMasterData.Core":
                    formToolbarActionList.HtmlTemplateActions.Add(
                        JsonSerializer.Deserialize<HtmlTemplateAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.InternalAction, JJMasterData.Core":
                    formToolbarActionList.InternalActions.Add(
                        JsonSerializer.Deserialize<InternalAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ScriptAction, JJMasterData.Core":
                    formToolbarActionList.JsActions.Add(
                        JsonSerializer.Deserialize<ScriptAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SqlCommandAction, JJMasterData.Core":
                    formToolbarActionList.SqlActions.Add(
                        JsonSerializer.Deserialize<SqlCommandAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.UrlRedirectAction, JJMasterData.Core":
                    formToolbarActionList.UrlActions.Add(
                        JsonSerializer.Deserialize<UrlRedirectAction>(actionElement.GetRawText()));
                    break;
            }
        }

        return formToolbarActionList;
    }

    protected override FormToolbarActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options)
    {
        var formToolbarActionList = new FormToolbarActionList
        {
            SaveAction = JsonSerializer.Deserialize<SaveAction>(rootElement.GetProperty("saveAction").GetRawText(), options),
            BackAction = JsonSerializer.Deserialize<BackAction>(rootElement.GetProperty("backAction").GetRawText(), options),
            CancelAction = JsonSerializer.Deserialize<CancelAction>(rootElement.GetProperty("cancelAction").GetRawText(), options),
            FormEditAction = JsonSerializer.Deserialize<FormEditAction>(rootElement.GetProperty("formEditAction").GetRawText(), options),
            AuditLogFormToolbarAction = JsonSerializer.Deserialize<AuditLogFormToolbarAction>(rootElement.GetProperty("auditLogFormToolbarAction").GetRawText(), options)
        };
        return formToolbarActionList;
    }

    protected override void WriteActions(Utf8JsonWriter writer, FormToolbarActionList actionListToWrite, JsonSerializerOptions options)
    {
        WriteProperty(writer, "saveAction", actionListToWrite.SaveAction, options);
        WriteProperty(writer, "backAction", actionListToWrite.BackAction, options);
        WriteProperty(writer, "cancelAction", actionListToWrite.CancelAction, options);
        WriteProperty(writer, "formEditAction", actionListToWrite.FormEditAction, options);
        WriteProperty(writer, "auditLogFormToolbarAction", actionListToWrite.AuditLogFormToolbarAction, options);
    }
}