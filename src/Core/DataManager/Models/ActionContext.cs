using System;
using System.Collections;
using System.Collections.Generic;
using JJMasterData.Core.DataDictionary;
using JJMasterData.Core.FormEvents.Args;

namespace JJMasterData.Core.DataManager;

public record ActionContext(
    FormStateData FormStateData,
    ActionSource Source,
    EventHandler<ActionEventArgs> OnRenderAction
)
{
    public FormStateData FormStateData { get; } = FormStateData;
    public ActionSource Source { get; } = Source;
    public EventHandler<ActionEventArgs> OnRenderAction { get; } = OnRenderAction;
}