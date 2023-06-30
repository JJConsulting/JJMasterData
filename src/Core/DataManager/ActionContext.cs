using System;
using System.Collections;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager;

public record ActionContext(
    IDictionary<string, dynamic> Values,
    PageState PageState,
    ActionSource Source,
    EventHandler<ActionEventArgs> OnRenderAction
)
{
    public PageState PageState { get; } = PageState;
    public IDictionary<string, dynamic> Values { get; } = Values;
    public ActionSource Source { get; } = Source;
    public EventHandler<ActionEventArgs> OnRenderAction { get; } = OnRenderAction;
}