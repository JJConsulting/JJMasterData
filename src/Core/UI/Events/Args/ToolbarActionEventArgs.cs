using System;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.UI.Events.Args;

public class ToolbarActionEventArgs(BasicAction action, JJLinkButton linkButton)
    : EventArgs
{
    public string ActionName { get; init; } = action.Name ?? throw new ArgumentNullException(nameof(action));

    public JJLinkButton LinkButton { get; set; } = linkButton;

    public string HtmlResult { get; set; } = null;
}
