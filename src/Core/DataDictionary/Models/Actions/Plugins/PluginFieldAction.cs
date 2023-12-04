#nullable enable
using System.Collections.Generic;
using System.ComponentModel;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginFieldAction : PluginAction
{
    [DisplayName("Auto Trigger On Change")]
    public bool AutoTriggerOnChange { get; set; }
    public IDictionary<string, string> FieldMap { get;  } = new Dictionary<string,string>();
}