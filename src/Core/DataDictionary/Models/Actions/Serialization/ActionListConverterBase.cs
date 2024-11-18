using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

internal abstract class ActionListConverterBase<TActionList> : JsonConverter<TActionList>
    where TActionList : FormElementActionList, new()
{
    public override TActionList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var rootElement = doc.RootElement;

        if (rootElement.ValueKind == JsonValueKind.Array)
        {
            return ReadActionsFromLegacyFormat(rootElement);
        }

        var actionList = ReadActions(rootElement, options);

        actionList.AddRange(rootElement.GetProperty("sqlActions").Deserialize<List<SqlCommandAction>>(options));
        actionList.AddRange(rootElement.GetProperty("urlActions").Deserialize<List<UrlRedirectAction>>(options));
        actionList.AddRange(rootElement.GetProperty("htmlTemplateActions").Deserialize<List<HtmlTemplateAction>>(options));
        actionList.AddRange(rootElement.GetProperty("jsActions").Deserialize<List<ScriptAction>>(options));
        actionList.AddRange(rootElement.GetProperty("internalRedirectActions").Deserialize<List<InternalAction>>(options));
        actionList.AddRange(rootElement.GetProperty("pluginActions").Deserialize<List<PluginAction>>(options));
        return actionList;
    }

    public override void Write(Utf8JsonWriter writer, TActionList objectToWrite, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        WriteActionList(writer, "sqlActions", objectToWrite.SqlActions, options);
        WriteActionList(writer, "urlActions", objectToWrite.UrlActions, options);
        WriteActionList(writer, "htmlTemplateActions", objectToWrite.HtmlTemplateActions, options);
        WriteActionList(writer, "jsActions", objectToWrite.JsActions, options);
        WriteActionList(writer, "pluginActions", objectToWrite.PluginActions, options);
        WriteActionList(writer, "internalRedirectActions", objectToWrite.InternalActions, options);

        WriteActions(writer, objectToWrite, options);

        writer.WriteEndObject();
    }

    private static void WriteActionList<TAction>(Utf8JsonWriter writer, string propertyName,
        List<TAction> actionListToWrite, JsonSerializerOptions options)
    {
        writer.WriteStartArray(propertyName);
        foreach (var action in actionListToWrite)
        {
            JsonSerializer.Serialize(writer, action, options);
        }

        writer.WriteEndArray();
    }

    protected static void WriteProperty<T>(Utf8JsonWriter writer, string propertyName, T value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(propertyName);
        JsonSerializer.Serialize(writer, value, options);
    }

    protected abstract TActionList ReadActionsFromLegacyFormat(JsonElement actionElement);
    protected abstract TActionList ReadActions(JsonElement rootElement, JsonSerializerOptions options);
    protected abstract void WriteActions(Utf8JsonWriter writer, TActionList actionListToWrite,
        JsonSerializerOptions options);
}