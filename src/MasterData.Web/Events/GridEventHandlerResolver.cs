#nullable disable warnings
using System;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Web.Events.Abstractions;

namespace JJMasterData.Web.Events;

internal sealed class GridEventHandlerResolver(IServiceProvider serviceProvider)
    : EventHandlerResolverBase<IGridEventHandler>(serviceProvider),
        IGridEventHandlerResolver
{
    public IGridEventHandler GetGridEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}