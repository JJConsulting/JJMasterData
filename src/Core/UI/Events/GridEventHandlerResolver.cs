using System.Collections.Generic;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;

namespace JJMasterData.Core.UI.Events;

public class GridEventHandlerResolver
    (IEnumerable<IGridEventHandler> eventHandlers) : EventHandlerResolverBase<IGridEventHandler>(eventHandlers),
        IGridEventHandlerResolver
{
    public IGridEventHandler GetGridEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}