using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JJMasterData.Commons.DI;
using JJMasterData.Core.FormEvents.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace JJMasterData.Core.FormEvents;

/// <summary>
/// Default implementation of IFormEventResolver. 
/// </summary>
public class FormEventResolver : IFormEventResolver
{
    private IServiceScopeFactory ServiceScopeFactory { get; }
    private IEnumerable<Assembly> Assemblies { get; }

    public FormEventResolver(IOptions<FormEventOptions> options, IServiceScopeFactory serviceScopeFactory)
    {
        ServiceScopeFactory = serviceScopeFactory;
        Assemblies = options.Value.Assemblies;
    }

    public IFormEvent GetFormEvent(string elementName)
    {
        var assemblies = new List<Assembly>
        {
#if NET
            Assembly.GetEntryAssembly()
#endif
        };

        if (Assemblies?.Any() ?? false)
            assemblies.AddRange(Assemblies);

        var formEventType = GetFormEventTypes(assemblies)
            .FirstOrDefault(info =>
                info.GetCustomAttribute<FormEventAttribute>()?.ElementName == elementName);

        if (formEventType == null) 
            return null;
        
        using var scope = ServiceScopeFactory.CreateScope();
        return (IFormEvent)ActivatorUtilities.CreateInstance(scope.ServiceProvider, formEventType);
    }

    private static IEnumerable<TypeInfo> GetFormEventTypes(IEnumerable<Assembly> assemblies)
    {
        return assemblies.SelectMany(a => a?.DefinedTypes.Where(x =>
            x.GetInterfaces().Any(i => i == typeof(IFormEvent)))).ToList();
    }
}