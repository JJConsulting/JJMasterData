using System;
using JetBrains.Annotations;
using JJMasterData.Core.Events;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events;
using JJMasterData.Core.UI.Events.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JJMasterData.Core.Configuration;

[PublicAPI]
public static class EventHandlerServiceExtensions
{
    extension(IServiceCollection services)
    {
        internal void AddEventHandlers()
        {
            services.TryAddScoped<IFormEventHandlerResolver, FormEventHandlerResolver>();
            services.TryAddScoped<IGridEventHandlerResolver, GridEventHandlerResolver>();
        }

        public IServiceCollection AddFormEventHandler<TEventHandler>(string elementName, ServiceLifetime lifetime = ServiceLifetime.Transient)
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

        public IServiceCollection AddGridEventHandler<TEventHandler>(string elementName, ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TEventHandler : class, IGridEventHandler
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddKeyedSingleton<IGridEventHandler, TEventHandler>(elementName);
                    break;
                case ServiceLifetime.Scoped:
                    services.AddKeyedScoped<IGridEventHandler, TEventHandler>(elementName);
                    break;
                case ServiceLifetime.Transient:
                    services.AddKeyedTransient<IGridEventHandler, TEventHandler>(elementName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }

            return services;
        }
    }
}