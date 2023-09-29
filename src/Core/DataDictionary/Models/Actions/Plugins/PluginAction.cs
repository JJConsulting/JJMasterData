#nullable enable

using System;
using System.Collections.Generic;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginAction : UserCreatedAction
{
    public required Guid PluginId { get; init; }
    
    public bool AutoReloadFormFields { get; set; }
    public IDictionary<string,object?>? AdditionalParameters { get; set; }
    
    public PluginAction()
    {
        ShowAsButton = true;
        Text = "Plugin";
        Icon = IconType.Plug;
        AdditionalParameters = new Dictionary<string, object?>();
    }
}