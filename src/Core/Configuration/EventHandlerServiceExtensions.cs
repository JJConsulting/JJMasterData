using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Events;
using JJMasterData.Core.Events.Abstractions;
using JJMasterData.Core.UI.Events;
using JJMasterData.Core.UI.Events.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace JJMasterData.Core.Configuration;

public static class EventHandlerServiceExtensions
{
    public static IServiceCollection AddEventHandlers(this IServiceCollection services)
    {
        services.AddScoped<IFormEventHandlerResolver,FormEventHandlerResolver>();
        services.AddScoped<IGridEventHandlerResolver,GridEventHandlerResolver>();
        
        return services;
    }

    public static IServiceCollection AddEventHandlers(this IServiceCollection services, params Assembly[] additionalAssemblies)
    {
        var assemblies = new List<Assembly>
        {
            typeof(EventHandlerServiceExtensions).Assembly
        };

        assemblies.AddRange(additionalAssemblies);
        
        var types = ReflectionUtils.GetDefinedTypes<IEventHandler>(assemblies);

        foreach (var eventType in types.Where(t=>!t.IsAbstract))
        {
            if (eventType.ImplementedInterfaces.Contains(typeof(IFormEventHandler)))
            {
                services.Add(new ServiceDescriptor(typeof(IFormEventHandler), eventType, ServiceLifetime.Transient));
            }

            if (eventType.ImplementedInterfaces.Contains(typeof(IGridEventHandler)))
            {
                services.Add(new ServiceDescriptor(typeof(IGridEventHandler), eventType, ServiceLifetime.Transient));
            }
        }

        return services;
    }
}