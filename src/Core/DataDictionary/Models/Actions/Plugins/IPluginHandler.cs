#nullable enable
using System;
using System.Collections.Generic;
using JJMasterData.Core.UI.Html;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public interface IPluginHandler
{
    /// <summary>
    /// Unique identifier of the plugin.
    /// </summary>
    public Guid Id { get; }
    
    public string Title { get; }
    
    /// <summary>
    /// Optional additional parameters keys of the action at runtime.
    /// Examples: Procedure names, URLs, API keys
    /// </summary>
    public IEnumerable<PluginConfigurationField>? ConfigurationFields { get; }
    
    /// <summary>
    /// Optional information about the plugin that will be shown to the developer.
    /// </summary>
    public HtmlBuilder? AdditionalInformationHtml { get; }
    
}