using System.Collections.Generic;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.FormEvents.Abstractions;

namespace JJMasterData.Core.DataDictionary.FormEvents;

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