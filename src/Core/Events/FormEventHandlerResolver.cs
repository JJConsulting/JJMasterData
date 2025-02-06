using System;
using JJMasterData.Core.Events.Abstractions;

namespace JJMasterData.Core.Events;

internal sealed class FormEventHandlerResolver(IServiceProvider serviceProvider)
    : EventHandlerResolverBase<IFormEventHandler>(serviceProvider), IFormEventHandlerResolver
{
    public IFormEventHandler GetFormEventHandler(string elementName)
    {
        return GetEventHandler(elementName);
    }
}