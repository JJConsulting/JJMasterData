using System;
using JetBrains.Annotations;
using JJMasterData.Core.Events;
using JJMasterData.Core.Events.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

[PublicAPI]
public static class EventHandlerServiceExtensions
{
    internal static void AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IFormEventHandlerResolver, FormEventHandlerResolver>();
    }

    public static IServiceCollection AddFormEventHandler<TEventHandler>(this IServiceCollection services,
        string elementName, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TEventHandler : class, IFormEventHandler
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddKeyedSingleton<IFormEventHandler, TEventHandler>(elementName);
                break;
            case ServiceLifetime.Scoped:
                services.AddKeyedScoped<IFormEventHandler, TEventHandler>(elementName);
                break;
            case ServiceLifetime.Transient:
                services.AddKeyedTransient<IFormEventHandler, TEventHandler>(elementName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }

        return services;
    }

}
