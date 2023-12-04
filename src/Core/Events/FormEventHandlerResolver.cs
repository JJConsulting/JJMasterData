using System.Collections.Generic;
using JJMasterData.Core.Events.Abstractions;

namespace JJMasterData.Core.Events;

public class FormEventHandlerResolver
    (IEnumerable<IFormEventHandler> eventHandlers) : EventHandlerResolverBase<IFormEventHandler>(eventHandlers),
        IFormEventHandlerResolver
{
    public IFormEventHandler GetFormEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}