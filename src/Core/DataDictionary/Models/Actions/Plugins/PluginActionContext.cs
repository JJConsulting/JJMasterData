#nullable enable
using System.Collections.Generic;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginActionContext
{
    public required ActionContext ActionContext { get; init; }
    public required IDictionary<string,object?> ConfigurationMap { get; init; }
    public IDictionary<string, object?> Values => ActionContext.FormStateData.Values;
}