namespace JJMasterData.Core.DataDictionary.Models.Actions;

using System.Text.Json;

internal sealed class GridToolbarActionListConverter : ActionListConverterBase<GridToolbarActionList>
{
    protected override GridToolbarActionList ReadActionsFromLegacyFormat(JsonElement rootElement)
    {
        var gridToolbarActionList = new GridToolbarActionList();
        foreach (var actionElement in rootElement.EnumerateArray())
        {
            var type = actionElement.GetProperty("$type").GetString();

            switch (type)
            {
                case "JJMasterData.Core.DataDictionary.Models.Actions.InsertAction, JJMasterData.Core":
                    gridToolbarActionList.InsertAction =
                        JsonSerializer.Deserialize<InsertAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.LegendAction, JJMasterData.Core":
                    gridToolbarActionList.LegendAction =
                        JsonSerializer.Deserialize<LegendAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.RefreshAction, JJMasterData.Core":
                    gridToolbarActionList.RefreshAction =
                        JsonSerializer.Deserialize<RefreshAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.FilterAction, JJMasterData.Core":
                    gridToolbarActionList.FilterAction =
                        JsonSerializer.Deserialize<FilterAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ImportAction, JJMasterData.Core":
                    gridToolbarActionList.ImportAction =
                        JsonSerializer.Deserialize<ImportAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ExportAction, JJMasterData.Core":
                    gridToolbarActionList.ExportAction =
                        JsonSerializer.Deserialize<ExportAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ConfigAction, JJMasterData.Core":
                    gridToolbarActionList.ConfigAction =
                        JsonSerializer.Deserialize<ConfigAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SortAction, JJMasterData.Core":
                    gridToolbarActionList.SortAction =
                        JsonSerializer.Deserialize<SortAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.AuditLogGridToolbarAction, JJMasterData.Core":
                    gridToolbarActionList.AuditLogGridToolbarAction =
                        JsonSerializer.Deserialize<AuditLogGridToolbarAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.HtmlTemplateAction, JJMasterData.Core":
                    gridToolbarActionList.HtmlTemplateActions.Add(
                        JsonSerializer.Deserialize<HtmlTemplateAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.InternalAction, JJMasterData.Core":
                    gridToolbarActionList.InternalActions.Add(
                        JsonSerializer.Deserialize<InternalAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ScriptAction, JJMasterData.Core":
                    gridToolbarActionList.JsActions.Add(
                        JsonSerializer.Deserialize<ScriptAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SqlCommandAction, JJMasterData.Core":
                    gridToolbarActionList.SqlActions.Add(
                        JsonSerializer.Deserialize<SqlCommandAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.UrlRedirectAction, JJMasterData.Core":
                    gridToolbarActionList.UrlActions.Add(
                        JsonSerializer.Deserialize<UrlRedirectAction>(actionElement.GetRawText()));
                    break;
            }
        }

        return gridToolbarActionList;
    }

    protected override GridToolbarActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options)
    {
        var gridToolbarActionList = new GridToolbarActionList
        {
            InsertAction =
                JsonSerializer.Deserialize<InsertAction>(rootElement.GetProperty("insertAction").GetRawText(), options),
            LegendAction =
                JsonSerializer.Deserialize<LegendAction>(rootElement.GetProperty("legendAction").GetRawText(), options),
            RefreshAction =
                JsonSerializer.Deserialize<RefreshAction>(rootElement.GetProperty("refreshAction").GetRawText(),
                    options),
            FilterAction =
                JsonSerializer.Deserialize<FilterAction>(rootElement.GetProperty("filterAction").GetRawText(), options),
            ImportAction =
                JsonSerializer.Deserialize<ImportAction>(rootElement.GetProperty("importAction").GetRawText(), options),
            ExportAction =
                JsonSerializer.Deserialize<ExportAction>(rootElement.GetProperty("exportAction").GetRawText(), options),
            ConfigAction =
                JsonSerializer.Deserialize<ConfigAction>(rootElement.GetProperty("configAction").GetRawText(), options),
            SortAction =
                JsonSerializer.Deserialize<SortAction>(rootElement.GetProperty("sortAction").GetRawText(), options),
            AuditLogGridToolbarAction =
                JsonSerializer.Deserialize<AuditLogGridToolbarAction>(
                    rootElement.GetProperty("auditLogGridToolbarAction").GetRawText(), options)
        };
        return gridToolbarActionList;
    }

    protected override void WriteActions(Utf8JsonWriter writer, GridToolbarActionList actionListToWrite,
        JsonSerializerOptions options)
    {
        WriteProperty(writer, "insertAction", actionListToWrite.InsertAction, options);
        WriteProperty(writer, "legendAction", actionListToWrite.LegendAction, options);
        WriteProperty(writer, "refreshAction", actionListToWrite.RefreshAction, options);
        WriteProperty(writer, "filterAction", actionListToWrite.FilterAction, options);
        WriteProperty(writer, "importAction", actionListToWrite.ImportAction, options);
        WriteProperty(writer, "exportAction", actionListToWrite.ExportAction, options);
        WriteProperty(writer, "configAction", actionListToWrite.ConfigAction, options);
        WriteProperty(writer, "sortAction", actionListToWrite.SortAction, options);
        WriteProperty(writer, "auditLogGridToolbarAction", actionListToWrite.AuditLogGridToolbarAction, options);
    }
}