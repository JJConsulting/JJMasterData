#nullable enable
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginActionContext
{
    public required IDictionary<string,object?> AdditionalParameters { get; init; }
    public required IDictionary<string,object?> Values { get; init; }
    
    /// <summary>
    /// Name of the field that triggered the action
    /// </summary>
    public string? TriggeredFieldName { get; init; }
}