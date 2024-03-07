#nullable enable
using System.Collections.Generic;
using System.ComponentModel;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginFieldAction : PluginAction
{
    [DisplayName("Trigger On Change")]
    public bool TriggerOnChange { get; set; }
    public Dictionary<string, string> FieldMap { get; private set; } = new();
    
    public override BasicAction DeepCopy()
    {
        var newAction = (PluginFieldAction)CopyAction();
        newAction.ConfigurationMap = new Dictionary<string, object?>(ConfigurationMap);
        newAction.FieldMap = new Dictionary<string, string>(FieldMap);
        newAction.TriggerOnChange = TriggerOnChange;
        return newAction;
    }
}