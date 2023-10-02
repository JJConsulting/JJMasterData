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
        services.AddScoped<IFormEventHandlerFactory,FormEventHandlerFactory>();
        services.AddScoped<IGridEventHandlerFactory,GridEventHandlerFactory>();

        services.AddEventHandlers<IFormEventHandler>();
        services.AddEventHandlers<IGridEventHandler>();
        
        return services;
    }

    private static IServiceCollection AddEventHandlers<T>(this IServiceCollection services) where T : IEventHandler
    {
        var assemblies = new List<Assembly>
        {
            Assembly.GetEntryAssembly(),
            typeof(EventHandlerServiceExtensions).Assembly
        };
        
        var types = ReflectionUtils.GetDefinedTypes<T>(assemblies);

        foreach (var formEventHandlerType in types.Where(t=>!t.IsAbstract))
        {
            services.Add(new ServiceDescriptor(typeof(T), formEventHandlerType, ServiceLifetime.Transient));
        }

        return services;
    }
}