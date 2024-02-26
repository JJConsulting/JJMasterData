#nullable enable
using System.Collections.Generic;
using System.ComponentModel;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginFieldAction : PluginAction
{
    [DisplayName("Trigger On Change")]
    public bool TriggerOnChange { get; set; }
    public Dictionary<string, string> FieldMap { get;  } = new();
}