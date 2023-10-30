using System.Collections.Generic;
using JJMasterData.Core.Events.Abstractions;

namespace JJMasterData.Core.Events;

public class FormEventHandlerResolver : EventHandlerResolverBase<IFormEventHandler>, IFormEventHandlerResolver
{
    public FormEventHandlerResolver(IEnumerable<IFormEventHandler> eventHandlers) : base(eventHandlers)
    {
    }

    public IFormEventHandler GetFormEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}