using System.Text.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

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
                    gridToolbarActionList.InsertAction = actionElement.Deserialize<InsertAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.LegendAction, JJMasterData.Core":
                    gridToolbarActionList.LegendAction = actionElement.Deserialize<LegendAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.RefreshAction, JJMasterData.Core":
                    gridToolbarActionList.RefreshAction = actionElement.Deserialize<RefreshAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.FilterAction, JJMasterData.Core":
                    gridToolbarActionList.FilterAction = actionElement.Deserialize<FilterAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ImportAction, JJMasterData.Core":
                    gridToolbarActionList.ImportAction = actionElement.Deserialize<ImportAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ExportAction, JJMasterData.Core":
                    gridToolbarActionList.ExportAction = actionElement.Deserialize<ExportAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ConfigAction, JJMasterData.Core":
                    gridToolbarActionList.ConfigAction = actionElement.Deserialize<ConfigAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SortAction, JJMasterData.Core":
                    gridToolbarActionList.SortAction = actionElement.Deserialize<SortAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.AuditLogGridToolbarAction, JJMasterData.Core":
                    gridToolbarActionList.AuditLogGridToolbarAction =
                        actionElement.Deserialize<AuditLogGridToolbarAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.PluginAction, JJMasterData.Core":
                    gridToolbarActionList.Set(actionElement.Deserialize<PluginAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.HtmlTemplateAction, JJMasterData.Core":
                    gridToolbarActionList.Set(actionElement.Deserialize<HtmlTemplateAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.InternalAction, JJMasterData.Core":
                    gridToolbarActionList.Set(actionElement.Deserialize<InternalAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ScriptAction, JJMasterData.Core":
                    gridToolbarActionList.Set(actionElement.Deserialize<ScriptAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SqlCommandAction, JJMasterData.Core":
                    gridToolbarActionList.Set(actionElement.Deserialize<SqlCommandAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.UrlRedirectAction, JJMasterData.Core":
                    gridToolbarActionList.Set(actionElement.Deserialize<UrlRedirectAction>());
                    break;
            }
        }

        return gridToolbarActionList;
    }

    protected override GridToolbarActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options)
    {
        var gridToolbarActionList = new GridToolbarActionList
        {
            InsertAction = rootElement.GetProperty("insertAction").Deserialize<InsertAction>(options),
            GridSaveAction = rootElement.TryGetProperty("gridSaveAction", out var gridSaveAction)
                ? gridSaveAction.Deserialize<GridSaveAction>(options)
                : new(),
            GridCancelAction = rootElement.TryGetProperty("gridCancelAction", out var gridCancelAction)
                ? gridCancelAction.Deserialize<GridCancelAction>(options)
                : new(),
            GridEditAction = rootElement.TryGetProperty("gridEditAction", out var gridEditAction)
                ? gridEditAction.Deserialize<GridEditAction>(options)
                : new(),
            LegendAction = rootElement.GetProperty("legendAction").Deserialize<LegendAction>(options),
            RefreshAction = rootElement.GetProperty("refreshAction").Deserialize<RefreshAction>(options),
            FilterAction = rootElement.GetProperty("filterAction").Deserialize<FilterAction>(options),
            ImportAction = rootElement.GetProperty("importAction").Deserialize<ImportAction>(options),
            ExportAction = rootElement.GetProperty("exportAction").Deserialize<ExportAction>(options),
            ConfigAction = rootElement.GetProperty("configAction").Deserialize<ConfigAction>(options),
            SortAction = rootElement.GetProperty("sortAction").Deserialize<SortAction>(options),
            AuditLogGridToolbarAction = rootElement.GetProperty("auditLogGridToolbarAction")
                .Deserialize<AuditLogGridToolbarAction>(options)
        };
        return gridToolbarActionList;
    }

    protected override void WriteActions(Utf8JsonWriter writer, GridToolbarActionList actionListToWrite,
        JsonSerializerOptions options)
    {
        WriteProperty(writer, "gridEditAction", actionListToWrite.GridEditAction, options);
        WriteProperty(writer, "gridSaveAction", actionListToWrite.GridSaveAction, options);
        WriteProperty(writer, "gridCancelAction", actionListToWrite.GridCancelAction, options);

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