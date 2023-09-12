using System.Collections.Generic;
using JJMasterData.Core.FormEvents;
using JJMasterData.Core.Web.FormEvents.Abstractions;

namespace JJMasterData.Core.Web.FormEvents.Factories;

public class GridEventHandlerFactory : EventHandlerFactoryBase<IGridEventHandler>, IGridEventHandlerFactory
{
    public GridEventHandlerFactory(IEnumerable<IGridEventHandler> eventHandlers) : base(eventHandlers)
    {
    }
    
    public IGridEventHandler GetGridEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}