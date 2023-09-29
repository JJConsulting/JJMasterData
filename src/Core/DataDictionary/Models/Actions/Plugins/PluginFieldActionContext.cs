#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginFieldActionContext : PluginActionContext
{
    public string? FieldName => ActionContext.FieldName;
    
    public required IDictionary<string,string> FieldMap { get; init; }
}