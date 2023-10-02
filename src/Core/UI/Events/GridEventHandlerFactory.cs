using System.Collections.Generic;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;

namespace JJMasterData.Core.UI.Events;

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