using System;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary.Models.Actions;
using JJMasterData.Core.UI.Components;

namespace JJMasterData.Core.UI.Events.Args;

public class ActionEventArgs : EventArgs
{
    [Obsolete("Please use ActionName property")]
    public BasicAction Action => throw new InvalidOperationException("Please use ActionName property.");
    
    public string ActionName { get; init; }
    
    public JJLinkButton LinkButton { get; set; }
    
    public IDictionary<string, object> FieldValues { get; internal set; }
    
    public string HtmlResult { get; set; }

    public ActionEventArgs(BasicAction action, JJLinkButton linkButton, IDictionary<string, object> fieldValues)
    {
        ActionName = action.Name ?? throw new ArgumentNullException(nameof(action));
        LinkButton = linkButton;
        FieldValues = fieldValues;
        HtmlResult = null;
    }
}
