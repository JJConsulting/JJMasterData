#nullable enable

using System;
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginAction : UserCreatedAction
{
    public required Guid PluginId { get; init; }
    
    public IDictionary<string, object> ConfigurationMap { get; } = new Dictionary<string,object>();
    
    public PluginAction()
    {
        ShowAsButton = true;
        Text = "Plugin";
        Icon = IconType.Plug;
    }
}