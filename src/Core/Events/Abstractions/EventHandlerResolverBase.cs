#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Events.Abstractions;

internal class EventHandlerResolverBase<TEventHandler>(IServiceProvider serviceProvider) where TEventHandler : IEventHandler
{
    protected TEventHandler? GetEventHandler(string elementName)
    {
        var eventHandler = serviceProvider.GetKeyedService<TEventHandler>(elementName);
        return eventHandler;
    }
}