#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJMasterData.Commons.DI;
using JJMasterData.Commons.Util;
using JJMasterData.Core.Configuration;
using JJMasterData.Core.FormEvents.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.FormEvents;


//TODO; I think IServiceScopeFactory is bad and we should inject IEnumerable<IFormEventHandler>
public class EventHandlerFactoryBase<T> where T : IEventHandler
{
    private IServiceScopeFactory ServiceScopeFactory { get; }
    private IEnumerable<Assembly?>? Assemblies { get; }

    public EventHandlerFactoryBase(IOptions<EventHandlerFactoryOptions> options, IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
        Assemblies = options.Value.Assemblies;
    }

    protected T? GetEventHandler(string elementName) 
    {
        var assemblies = new List<Assembly?>
        {
#if NET
            Assembly.GetEntryAssembly(),
            typeof(EventHandlerServiceExtensions).Assembly
#endif
        };

        if (Assemblies?.Any() ?? false)
            assemblies.AddRange(Assemblies);

        var types = ReflectionUtils.GetDefinedTypes<T>(assemblies);

        foreach (var formEventHandlerType in types.Where(t=>!t.IsAbstract))
        {
            using var scope = ServiceScopeFactory.CreateScope();
            var instance = (T)ActivatorUtilities.CreateInstance(scope.ServiceProvider, formEventHandlerType);

            if (instance.ElementName == elementName)
                return instance;

        }

        return default;
    }
}

public class EventHandlerFactoryOptions
{
    /// <summary>
    /// Default value: null <br></br>
    /// </summary>
    public IEnumerable<Assembly>? Assemblies { get; set; }

}