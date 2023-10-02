using System.Collections.Generic;
using JJMasterData.Core.Events.Abstractions;

namespace JJMasterData.Core.Events;

public class FormEventHandlerFactory : EventHandlerFactoryBase<IFormEventHandler>, IFormEventHandlerFactory
{
    public FormEventHandlerFactory(IEnumerable<IFormEventHandler> eventHandlers) : base(eventHandlers)
    {
    }

    public IFormEventHandler GetFormEvent(string elementName)
    {
        return GetEventHandler(elementName);
    }
}