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
                    gridTableActionList.DeleteAction =
                        JsonSerializer.Deserialize<DeleteAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.EditAction, JJMasterData.Core":
                    gridTableActionList.EditAction =
                        JsonSerializer.Deserialize<EditAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ViewAction, JJMasterData.Core":
                    gridTableActionList.ViewAction =
                        JsonSerializer.Deserialize<ViewAction>(actionElement.GetRawText());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.HtmlTemplateAction, JJMasterData.Core":
                    gridTableActionList.HtmlTemplateActions.Add(
                        JsonSerializer.Deserialize<HtmlTemplateAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.InternalAction, JJMasterData.Core":
                    gridTableActionList.InternalActions.Add(
                        JsonSerializer.Deserialize<InternalAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ScriptAction, JJMasterData.Core":
                    gridTableActionList.JsActions.Add(
                        JsonSerializer.Deserialize<ScriptAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SqlCommandAction, JJMasterData.Core":
                    gridTableActionList.SqlActions.Add(
                        JsonSerializer.Deserialize<SqlCommandAction>(actionElement.GetRawText()));
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.UrlRedirectAction, JJMasterData.Core":
                    gridTableActionList.UrlActions.Add(
                        JsonSerializer.Deserialize<UrlRedirectAction>(actionElement.GetRawText()));
                    break;
            }
        }

        return gridTableActionList;
    }

    protected override GridTableActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options)
    {
        var gridTableActionList = new GridTableActionList
        {
            DeleteAction = JsonSerializer.Deserialize<DeleteAction>(rootElement.GetProperty("deleteAction").GetRawText(), options),
            EditAction = JsonSerializer.Deserialize<EditAction>(rootElement.GetProperty("editAction").GetRawText(), options),
            ViewAction = JsonSerializer.Deserialize<ViewAction>(rootElement.GetProperty("viewAction").GetRawText(), options)
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