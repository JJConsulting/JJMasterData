#nullable enable
using System.Collections.Generic;
using System.Linq;
using JJMasterData.Core.FormEvents.Abstractions;

namespace JJMasterData.Core.FormEvents;

public class EventHandlerFactoryBase<TEventHandler> where TEventHandler : IEventHandler
{
    private IEnumerable<TEventHandler> EventHandlers { get; }

    protected EventHandlerFactoryBase(IEnumerable<TEventHandler> eventHandlers)
    {
        EventHandlers = eventHandlers;
    }

    protected TEventHandler? GetEventHandler(string elementName)
    {
        return EventHandlers.FirstOrDefault(e => e.ElementName == elementName);
    }
}