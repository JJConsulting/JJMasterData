#nullable enable


using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.DataDictionary.Models.Actions;

public class PluginActionResult
{
    public string? JsCallback { get; init; }

    public static PluginActionResult Success()
    {
        return new PluginActionResult();
    }
    
    public static PluginActionResult Error(string? title = null, string? message = null)
    {
        var messageBox = new JJMessageBox
        {
            Title = title ?? "Error",
            Content = message,
            Size = MessageSize.Default,
            Icon = MessageIcon.Error
        };

        return new PluginActionResult
        {
            JsCallback = messageBox.GetShowScript()
        };
    }
}