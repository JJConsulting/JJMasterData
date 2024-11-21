#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using JJMasterData.Commons.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginAction : BasicAction
{
    [JsonPropertyName("pluginId")]
    public required Guid PluginId { get; init; }
    
    [JsonPropertyName("configurationMap")]
    [JsonConverter(typeof(DictionaryStringObjectJsonConverter))]
    [JsonInclude]
    public Dictionary<string, object?> ConfigurationMap { get; protected set; } = new();
    
    public PluginAction()
    {
        ShowAsButton = true;
        Text = "Plugin";
        Icon = IconType.Plug;
    }

    public override bool IsCustomAction => true;

    public override BasicAction DeepCopy()
    {
        var newAction = (PluginAction)MemberwiseClone();
        newAction.ConfigurationMap = new Dictionary<string, object?>(ConfigurationMap);
        return newAction;
    }
    
}