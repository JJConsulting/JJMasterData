#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginFieldAction : PluginAction
{
    public bool AutoTriggerOnChange { get; set; }
    public IDictionary<string, string> FieldMap { get; set; } = new Dictionary<string,string>();
}