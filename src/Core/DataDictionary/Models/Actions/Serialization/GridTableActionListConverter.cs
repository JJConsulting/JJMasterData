using System.Text.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

internal sealed class GridTableActionListConverter : ActionListConverterBase<GridTableActionList>
{
    protected override GridTableActionList ReadActionsFromLegacyFormat(JsonElement rootElement)
    {
        var gridTableActionList = new GridTableActionList();
        foreach (var actionElement in rootElement.EnumerateArray())
        {
            var type = actionElement.GetProperty("$type").GetString();
            switch (type)
            {
                case "JJMasterData.Core.DataDictionary.Models.Actions.DeleteAction, JJMasterData.Core":
                    gridTableActionList.DeleteAction = actionElement.Deserialize<DeleteAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.EditAction, JJMasterData.Core":
                    gridTableActionList.EditAction = actionElement.Deserialize<EditAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ViewAction, JJMasterData.Core":
                    gridTableActionList.ViewAction = actionElement.Deserialize<ViewAction>();
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.PluginAction, JJMasterData.Core":
                    gridTableActionList.PluginActions.Add(actionElement.Deserialize<PluginAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.HtmlTemplateAction, JJMasterData.Core":
                    gridTableActionList.HtmlTemplateActions.Add(actionElement.Deserialize<HtmlTemplateAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.InternalAction, JJMasterData.Core":
                    gridTableActionList.InternalActions.Add(actionElement.Deserialize<InternalAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ScriptAction, JJMasterData.Core":
                    gridTableActionList.JsActions.Add(actionElement.Deserialize<ScriptAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SqlCommandAction, JJMasterData.Core":
                    gridTableActionList.SqlActions.Add(actionElement.Deserialize<SqlCommandAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.UrlRedirectAction, JJMasterData.Core":
                    gridTableActionList.UrlActions.Add(actionElement.Deserialize<UrlRedirectAction>());
                    break;
            }
        }

        return gridTableActionList;
    }

    protected override GridTableActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options)
    {
        var gridTableActionList = new GridTableActionList
        {
            DeleteAction = rootElement.GetProperty("deleteAction").Deserialize<DeleteAction>(options),
            EditAction = rootElement.GetProperty("editAction").Deserialize<EditAction>(options),
            ViewAction = rootElement.GetProperty("viewAction").Deserialize<ViewAction>(options)
        };
        return gridTableActionList;
    }

    protected override void WriteActions(Utf8JsonWriter writer, GridTableActionList actionListToWrite, JsonSerializerOptions options)
    {
        WriteProperty(writer, "deleteAction", actionListToWrite.DeleteAction, options);
        WriteProperty(writer, "editAction", actionListToWrite.EditAction, options);
        WriteProperty(writer, "viewAction", actionListToWrite.ViewAction, options);
    }
}