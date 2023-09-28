#nullable enable
using JJMasterData.Core.Web.Components;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginActionModal
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    
    public MessageIcon Icon { get; set; }
    public MessageSize Size { get; set; }
    
    public string? Button1Label { get; set; }
    public string? Button2Label { get; set; }
    
    public BasicAction? Button1Action { get; set; }
    public BasicAction? Button2Action { get; set; }
}