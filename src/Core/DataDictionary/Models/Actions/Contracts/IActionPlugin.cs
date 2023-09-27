#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JJMasterData.Core.Web.Html;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public interface IActionPlugin
{
    /// <summary>
    /// Unique identifier of the plugin.
    /// </summary>
    public Guid Id { get; }
    
    public string Title { get; }
    
    /// <summary>
    /// Hint keywords to the additional parameters.
    /// </summary>
    public IEnumerable<string> AdditionalParametersHints { get; }
    
    /// <summary>
    /// Optional information about the plugin that will be shown to the developer.
    /// </summary>
    public HtmlBuilder? AdditionalInformationHtml { get; }
    
    public Task<PluginActionResult> ExecuteActionAsync(PluginActionContext pluginActionContext);
}
