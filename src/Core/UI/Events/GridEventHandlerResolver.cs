using System.Collections.Generic;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;

namespace JJMasterData.Core.UI.Events;

public class GridEventHandlerResolver : EventHandlerResolverBase<IGridEventHandler>, IGridEventHandlerResolver
{
    public GridEventHandlerResolver(IEnumerable<IGridEventHandler> eventHandlers) : base(eventHandlers)
    {
    }
    
    public IGridEventHandler GetGridEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}