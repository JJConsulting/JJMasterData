#nullable enable

using System;
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginAction : BasicAction
{
    public required Guid PluginId { get; init; }
    
    public Dictionary<string, object?> ConfigurationMap { get; protected set; } = new();
    
    public PluginAction()
    {
        ShowAsButton = true;
        Text = "Plugin";
        Icon = IconType.Plug;
    }

    public override bool IsCustomAction => true;

    public override BasicAction DeepCopy()
    {
        var newAction = (PluginAction)MemberwiseClone();
        newAction.ConfigurationMap = new Dictionary<string, object?>(ConfigurationMap);
        return newAction;
    }
    
}