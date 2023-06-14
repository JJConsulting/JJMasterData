using System;
using System.Collections;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager;

public record ActionContext(
    IDictionary Values, 
    PageState PageState, 
    ActionSource Source,
    EventHandler<ActionEventArgs> OnRenderAction
)
{
    public PageState PageState { get; } = PageState;
    public IDictionary Values { get; } = Values;
    public ActionSource Source { get; } = Source;
    public EventHandler<ActionEventArgs> OnRenderAction { get; } = OnRenderAction;
}