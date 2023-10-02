#nullable enable
using System;
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
    /// Optional information about the plugin that will be shown to the developer.
    /// </summary>
    public HtmlBuilder? AdditionalInformationHtml { get; }
    
}