using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.UI.Events.Args;

public class ActionEventArgs(BasicAction action, JJLinkButton linkButton, IDictionary<string, object> fieldValues)
    : EventArgs
{
    public string ActionName { get; init; } = action.Name ?? throw new ArgumentNullException(nameof(action));

    public JJLinkButton LinkButton { get; set; } = linkButton;

    public IDictionary<string, object> FieldValues { get; internal set; } = fieldValues;

    public string HtmlResult { get; set; } = null;
}
