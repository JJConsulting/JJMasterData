using System.Text.Json;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

internal sealed class FormElementFieldActionListConverter : ActionListConverterBase<FormElementFieldActionList>
{
    protected override FormElementFieldActionList ReadActionsFromLegacyFormat(JsonElement rootElement)
    {
        var fieldActionList = new FormElementFieldActionList();
        foreach (var actionElement in rootElement.EnumerateArray())
        {
            var type = actionElement.GetProperty("$type").GetString();
            switch (type)
            {
                case "JJMasterData.Core.DataDictionary.Models.Actions.PluginFieldAction, JJMasterData.Core":
                    fieldActionList.Set(actionElement.Deserialize<PluginFieldAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.PluginAction, JJMasterData.Core":
                    fieldActionList.Set(actionElement.Deserialize<PluginAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.HtmlTemplateAction, JJMasterData.Core":
                    fieldActionList.Set(actionElement.Deserialize<HtmlTemplateAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.InternalAction, JJMasterData.Core":
                    fieldActionList.Set(actionElement.Deserialize<InternalAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.ScriptAction, JJMasterData.Core":
                    fieldActionList.Set(actionElement.Deserialize<ScriptAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.SqlCommandAction, JJMasterData.Core":
                    fieldActionList.Set(actionElement.Deserialize<SqlCommandAction>());
                    break;
                case "JJMasterData.Core.DataDictionary.Models.Actions.UrlRedirectAction, JJMasterData.Core":
                    fieldActionList.Set(actionElement.Deserialize<UrlRedirectAction>());
                    break;
            }
        }

        return fieldActionList;
    }

    protected override FormElementFieldActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options)
    {
        var fieldActionList = new FormElementFieldActionList();

        if (!rootElement.TryGetProperty("pluginFieldActions", out var pluginFieldActionsElement))
            return fieldActionList;
        
        foreach (var actionElement in pluginFieldActionsElement.EnumerateArray())
            fieldActionList.Set(actionElement.Deserialize<PluginFieldAction>(options));
        
        return fieldActionList;
    }

    protected override void WriteActions(Utf8JsonWriter writer, FormElementFieldActionList actionListToWrite, JsonSerializerOptions options)
    {
        if (actionListToWrite.PluginFieldActions.Count == 0)
            return;
        
        writer.WriteStartArray("pluginFieldActions");
        
        foreach (var pluginFieldAction in actionListToWrite.PluginFieldActions)
            JsonSerializer.Serialize(writer, pluginFieldAction, options);
        
        writer.WriteEndArray();
    }
}
