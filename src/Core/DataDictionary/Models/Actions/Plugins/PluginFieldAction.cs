#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public sealed class PluginFieldAction : PluginAction
{
    [DisplayName("Trigger On Change")]
    [JsonPropertyName("triggerOnChange")]
    public bool TriggerOnChange { get; set; }
    
    [JsonPropertyName("fieldMap")]
    [JsonInclude]
    public Dictionary<string, string> FieldMap { get; private set; } = new();
    
    public override BasicAction DeepCopy()
    {
        var newAction = (PluginFieldAction)MemberwiseClone();
        newAction.ConfigurationMap = new Dictionary<string, object?>(ConfigurationMap);
        newAction.FieldMap = new Dictionary<string, string>(FieldMap);
        newAction.TriggerOnChange = TriggerOnChange;
        return newAction;
    }
}