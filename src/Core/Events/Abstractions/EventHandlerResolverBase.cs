#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace JJMasterData.Core.Events.Abstractions;

public class EventHandlerResolverBase<TEventHandler>(IEnumerable<TEventHandler> eventHandlers) where TEventHandler : IEventHandler
{
    protected TEventHandler? GetEventHandler(string elementName)
    {
        return eventHandlers.FirstOrDefault(e => e.ElementName == elementName);
    }
}