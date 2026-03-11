#nullable enable

using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginActionContext
{
    public required UI.Components.ActionContext ActionContext { get; init; }
    public required Dictionary<string,object?> ConfigurationMap { get; init; }
    public Dictionary<string, object?> Values => ActionContext.FormStateData.Values;
    public Dictionary<string, object?> SecretValues { get; } = new();
}