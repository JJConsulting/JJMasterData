using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public record PluginFieldActionContext : PluginActionContext
{
    public required Dictionary<string,string> FieldMap { get; init; }
}
