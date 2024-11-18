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
                    fieldActionList.PluginFieldActions.Add(actionElement.Deserialize<PluginFieldAction>());
                    break;
            }
        }

        return fieldActionList;
    }

    protected override FormElementFieldActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options)
    {
        var fieldActionList = new FormElementFieldActionList();
        var pluginFieldActionsElement = rootElement.GetProperty("pluginFieldActions");
        foreach (var actionElement in pluginFieldActionsElement.EnumerateArray())
        {
            fieldActionList.PluginFieldActions.Add(actionElement.Deserialize<PluginFieldAction>(options));
        }
        return fieldActionList;
    }

    protected override void WriteActions(Utf8JsonWriter writer, FormElementFieldActionList actionListToWrite, JsonSerializerOptions options)
    {
        writer.WriteStartArray("pluginFieldActions");
        foreach (var pluginFieldAction in actionListToWrite.PluginFieldActions)
        {
            JsonSerializer.Serialize(writer, pluginFieldAction, options);
        }
        writer.WriteEndArray();
    }
}
