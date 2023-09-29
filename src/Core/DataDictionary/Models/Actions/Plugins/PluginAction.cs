#nullable enable

using System;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginAction : UserCreatedAction
{
    public required Guid PluginId { get; init; }
    
    public PluginAction()
    {
        ShowAsButton = true;
        Text = "Plugin";
        Icon = IconType.Plug;
    }
}