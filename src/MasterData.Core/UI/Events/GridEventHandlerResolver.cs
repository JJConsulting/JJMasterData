using System;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events.Abstractions;

namespace JJMasterData.Core.UI.Events;

internal sealed class GridEventHandlerResolver(IServiceProvider serviceProvider)
    : EventHandlerResolverBase<IGridEventHandler>(serviceProvider),
        IGridEventHandlerResolver
{
    public IGridEventHandler GetGridEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}