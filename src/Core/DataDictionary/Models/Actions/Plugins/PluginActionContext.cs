#nullable enable
using System.Collections.Generic;
using ActionContext = JJMasterData.Core.UI.Components.ActionContext;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginActionContext
{
    public required ActionContext ActionContext { get; init; }
    public required Dictionary<string,object?> ConfigurationMap { get; init; }
    public Dictionary<string, object?> Values => ActionContext.FormStateData.Values;

    public Dictionary<string, object?> SecretValues { get; } = new();
}