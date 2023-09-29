#nullable enable
using System.Collections.Generic;
using JJMasterData.Core.DataManager.Models;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginActionContext
{
    public required ActionContext ActionContext { get; init; }
    public IDictionary<string, object?> Values => ActionContext.FormStateData.Values;
}